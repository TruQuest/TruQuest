// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingValidationVerifierLottery.sol";
import "./L1Block.sol";

error ThingValidationPoll__Unauthorized();
error ThingValidationPoll__Expired(bytes16 thingId);
error ThingValidationPoll__NotDesignatedVerifier(bytes16 thingId);
error ThingValidationPoll__NotActive(bytes16 thingId);
error ThingValidationPoll__StillInProgress(bytes16 thingId);

contract ThingValidationPoll {
    enum Vote {
        Decline__Soft,
        Decline__Hard,
        Accept
    }

    enum Decision {
        Unsettled__InsufficientVotingVolume,
        Unsettled__MajorityThresholdNotReached,
        Declined__Soft,
        Declined__Hard,
        Accepted
    }

    TruQuest private immutable i_truQuest;
    address private s_thingValidationVerifierLotteryAddress;
    address private s_orchestrator;

    L1Block private constant L1BLOCK =
        L1Block(0x4200000000000000000000000000000000000015);

    uint16 public s_durationBlocks;
    uint8 public s_votingVolumeThresholdPercent;
    uint8 public s_majorityThresholdPercent;

    mapping(bytes16 => int256) private s_thingIdToPollInitBlock;
    mapping(bytes16 => address[]) private s_thingVerifiers;

    event CastedVote(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote,
        uint256 l1BlockNumber
    );

    event CastedVoteWithReason(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote,
        string reason,
        uint256 l1BlockNumber
    );

    event PollFinalized(
        bytes16 indexed thingId,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] rewardedVerifiers,
        address[] slashedVerifiers
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert ThingValidationPoll__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert ThingValidationPoll__Unauthorized();
        }
        _;
    }

    modifier onlyThingValidationVerifierLottery() {
        if (msg.sender != s_thingValidationVerifierLotteryAddress) {
            revert ThingValidationPoll__Unauthorized();
        }
        _;
    }

    modifier whenActiveAndNotExpired(bytes16 _thingId) {
        int256 pollInitBlock = s_thingIdToPollInitBlock[_thingId];
        if (pollInitBlock < 1) {
            revert ThingValidationPoll__NotActive(_thingId);
        }
        if (_getL1BlockNumber() > uint256(pollInitBlock) + s_durationBlocks) {
            revert ThingValidationPoll__Expired(_thingId);
        }
        _;
    }

    modifier onlyDesignatedVerifier(
        bytes16 _thingId,
        uint16 _thingVerifiersArrayIndex
    ) {
        if (
            s_thingVerifiers[_thingId][_thingVerifiersArrayIndex] != msg.sender
        ) {
            revert ThingValidationPoll__NotDesignatedVerifier(_thingId);
        }
        _;
    }

    modifier whenActive(bytes16 _thingId) {
        if (s_thingIdToPollInitBlock[_thingId] < 1) {
            revert ThingValidationPoll__NotActive(_thingId);
        }
        _;
    }

    modifier whenExpired(bytes16 _thingId) {
        if (
            _getL1BlockNumber() <=
            uint256(s_thingIdToPollInitBlock[_thingId]) + s_durationBlocks
        ) {
            revert ThingValidationPoll__StillInProgress(_thingId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint16 _durationBlocks,
        uint8 _votingVolumeThresholdPercent,
        uint8 _majorityThresholdPercent
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = msg.sender;
        s_durationBlocks = _durationBlocks;
        s_votingVolumeThresholdPercent = _votingVolumeThresholdPercent;
        s_majorityThresholdPercent = _majorityThresholdPercent;
    }

    function setThingValidationVerifierLotteryAddress(
        address _thingValidationVerifierLotteryAddress
    ) external onlyOrchestrator {
        s_thingValidationVerifierLotteryAddress = _thingValidationVerifierLotteryAddress;
    }

    function _getL1BlockNumber() private view returns (uint256) {
        if (block.chainid == 901) {
            return L1BLOCK.number();
        }
        return block.number;
    }

    function getPollInitBlock(bytes16 _thingId) external view returns (int256) {
        return s_thingIdToPollInitBlock[_thingId];
    }

    function initPoll(
        bytes16 _thingId,
        address[] memory _verifiers
    ) external onlyThingValidationVerifierLottery {
        s_thingIdToPollInitBlock[_thingId] = int256(_getL1BlockNumber());
        s_thingVerifiers[_thingId] = _verifiers;
    }

    function castVote(
        bytes16 _thingId,
        uint16 _thingVerifiersArrayIndex,
        Vote _vote
    )
        external
        whenActiveAndNotExpired(_thingId)
        onlyDesignatedVerifier(_thingId, _thingVerifiersArrayIndex)
    {
        emit CastedVote(_thingId, msg.sender, _vote, _getL1BlockNumber());
    }

    function castVoteWithReason(
        bytes16 _thingId,
        uint16 _thingVerifiersArrayIndex,
        Vote _vote,
        string calldata _reason
    )
        external
        whenActiveAndNotExpired(_thingId)
        onlyDesignatedVerifier(_thingId, _thingVerifiersArrayIndex)
    {
        emit CastedVoteWithReason(
            _thingId,
            msg.sender,
            _vote,
            _reason,
            _getL1BlockNumber()
        );
    }

    function getVerifiers(
        bytes16 _thingId
    ) external view returns (address[] memory) {
        return s_thingVerifiers[_thingId];
    }

    function finalizePoll__Unsettled(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        Decision _decision,
        uint64[] calldata _verifiersToSlashIndices
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        s_thingIdToPollInitBlock[_thingId] = -s_thingIdToPollInitBlock[
            _thingId
        ];
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeThingSubmitter(submitter);

        uint64 j = 0;
        address[] memory verifiers = s_thingVerifiers[_thingId];
        address[] memory slashedVerifiers = new address[](
            _verifiersToSlashIndices.length
        );
        for (uint8 i = 0; i < _verifiersToSlashIndices.length; ++i) {
            uint64 nextVerifierToSlashIndex = _verifiersToSlashIndices[i];
            slashedVerifiers[i] = verifiers[nextVerifierToSlashIndex];
            for (; j < nextVerifierToSlashIndex; ++j) {
                i_truQuest.unstakeAsVerifier(verifiers[j]);
            }
            i_truQuest.unstakeAndSlashVerifier(verifiers[j++]);
        }
        for (; j < verifiers.length; ++j) {
            i_truQuest.unstakeAsVerifier(verifiers[j]);
        }

        emit PollFinalized(
            _thingId,
            _decision,
            _voteAggIpfsCid,
            submitter,
            new address[](0),
            slashedVerifiers
        );
    }

    function _rewardOrSlashVerifiers(
        bytes16 _thingId,
        uint64[] calldata _verifiersToSlashIndices
    )
        private
        returns (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        )
    {
        uint64 j = 0;
        address[] memory verifiers = s_thingVerifiers[_thingId];
        rewardedVerifiers = new address[](
            verifiers.length - _verifiersToSlashIndices.length
        );
        slashedVerifiers = new address[](_verifiersToSlashIndices.length);
        for (uint8 i = 0; i < _verifiersToSlashIndices.length; ++i) {
            uint64 nextVerifierToSlashIndex = _verifiersToSlashIndices[i];
            slashedVerifiers[i] = verifiers[nextVerifierToSlashIndex];
            for (; j < nextVerifierToSlashIndex; ++j) {
                rewardedVerifiers[j - i] = verifiers[j];
                i_truQuest.unstakeAndRewardVerifier(verifiers[j]);
            }
            i_truQuest.unstakeAndSlashVerifier(verifiers[j++]);
        }
        for (; j < verifiers.length; ++j) {
            rewardedVerifiers[j - slashedVerifiers.length] = verifiers[j];
            i_truQuest.unstakeAndRewardVerifier(verifiers[j]);
        }
    }

    function finalizePoll__Accepted(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        s_thingIdToPollInitBlock[_thingId] = -s_thingIdToPollInitBlock[
            _thingId
        ];
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeAndRewardThingSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingId, _verifiersToSlashIndices);

        emit PollFinalized(
            _thingId,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function finalizePoll__Declined__Soft(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        s_thingIdToPollInitBlock[_thingId] = -s_thingIdToPollInitBlock[
            _thingId
        ];
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeThingSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingId, _verifiersToSlashIndices);

        emit PollFinalized(
            _thingId,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function finalizePoll__Declined__Hard(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        s_thingIdToPollInitBlock[_thingId] = -s_thingIdToPollInitBlock[
            _thingId
        ];
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeAndSlashThingSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingId, _verifiersToSlashIndices);

        emit PollFinalized(
            _thingId,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function getUserIndexAmongThingVerifiers(
        bytes16 _thingId,
        address _user
    ) external view returns (int256) {
        int256 index = -1;
        address[] memory verifiers = s_thingVerifiers[_thingId];
        for (uint256 i = 0; i < verifiers.length; ++i) {
            if (verifiers[i] == _user) {
                index = int256(i);
                break;
            }
        }

        return index;
    }

    function checkUserIsThingsVerifierAtIndex(
        bytes16 _thingId,
        address _user,
        uint16 _index
    ) external view returns (bool) {
        return s_thingVerifiers[_thingId][_index] == _user;
    }
}
