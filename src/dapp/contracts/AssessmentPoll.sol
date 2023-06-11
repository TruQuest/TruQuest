// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingAssessmentVerifierLottery.sol";
import "./L1Block.sol";

error AssessmentPoll__Unauthorized();
error AssessmentPoll__Expired(bytes32 thingProposalId);
error AssessmentPoll__NotDesignatedVerifier(bytes32 thingProposalId);
error AssessmentPoll__NotActive(bytes32 thingProposalId);
error AssessmentPoll__StillInProgress(bytes32 thingProposalId);

contract AssessmentPoll {
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
    ThingAssessmentVerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    L1Block private constant L1BLOCK =
        L1Block(0x4200000000000000000000000000000000000015);

    uint16 private s_durationBlocks;
    uint8 private s_votingVolumeThresholdPercent;
    uint8 private s_majorityThresholdPercent;

    mapping(bytes32 => int256) private s_proposalIdToPollInitBlock;
    mapping(bytes32 => address[]) private s_proposalVerifiers;

    event CastedVote(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        Vote vote,
        uint256 l1BlockNumber
    );

    event CastedVoteWithReason(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        Vote vote,
        string reason,
        uint256 l1BlockNumber
    );

    event PollFinalized(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] rewardedVerifiers,
        address[] slashedVerifiers
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert AssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert AssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier onlyThingAssessmentVerifierLottery() {
        if (msg.sender != address(s_verifierLottery)) {
            revert AssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier whenActiveAndNotExpired(bytes32 _thingProposalId) {
        int256 pollInitBlock = s_proposalIdToPollInitBlock[_thingProposalId];
        if (pollInitBlock < 1) {
            revert AssessmentPoll__NotActive(_thingProposalId);
        }
        if (_getL1BlockNumber() > uint256(pollInitBlock) + s_durationBlocks) {
            revert AssessmentPoll__Expired(_thingProposalId);
        }
        _;
    }

    modifier onlyDesignatedVerifier(
        bytes32 _thingProposalId,
        uint16 _proposalVerifiersArrayIndex
    ) {
        if (
            s_proposalVerifiers[_thingProposalId][
                _proposalVerifiersArrayIndex
            ] != msg.sender
        ) {
            revert AssessmentPoll__NotDesignatedVerifier(_thingProposalId);
        }
        _;
    }

    modifier whenActive(bytes32 _thingProposalId) {
        if (s_proposalIdToPollInitBlock[_thingProposalId] < 1) {
            revert AssessmentPoll__NotActive(_thingProposalId);
        }
        _;
    }

    modifier whenExpired(bytes32 _thingProposalId) {
        if (
            _getL1BlockNumber() <=
            uint256(s_proposalIdToPollInitBlock[_thingProposalId]) +
                s_durationBlocks
        ) {
            revert AssessmentPoll__StillInProgress(_thingProposalId);
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
        s_orchestrator = tx.origin;
        s_durationBlocks = _durationBlocks;
        s_votingVolumeThresholdPercent = _votingVolumeThresholdPercent;
        s_majorityThresholdPercent = _majorityThresholdPercent;
    }

    function _splitIds(
        bytes32 _thingProposalId
    ) private pure returns (bytes16, bytes16) {
        return (
            bytes16(_thingProposalId),
            bytes16(uint128(uint256(_thingProposalId)))
        );
    }

    function connectToThingAssessmentVerifierLottery(
        address _verifierLotteryAddress
    ) external onlyTruQuest {
        s_verifierLottery = ThingAssessmentVerifierLottery(
            _verifierLotteryAddress
        );
    }

    function _getL1BlockNumber() private view returns (uint256) {
        if (block.chainid == 901) {
            return L1BLOCK.number();
        }
        return block.number;
    }

    function getPollDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getPollInitBlock(
        bytes32 _thingProposalId
    ) public view returns (int256) {
        return s_proposalIdToPollInitBlock[_thingProposalId];
    }

    function initPoll(
        bytes32 _thingProposalId,
        address[] memory _verifiers
    ) external onlyThingAssessmentVerifierLottery {
        s_proposalIdToPollInitBlock[_thingProposalId] = int256(
            _getL1BlockNumber()
        );
        s_proposalVerifiers[_thingProposalId] = _verifiers;
    }

    function castVote(
        bytes32 _thingProposalId,
        uint16 _proposalVerifiersArrayIndex,
        Vote _vote
    )
        public
        whenActiveAndNotExpired(_thingProposalId)
        onlyDesignatedVerifier(_thingProposalId, _proposalVerifiersArrayIndex)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        emit CastedVote(
            thingId,
            proposalId,
            msg.sender,
            _vote,
            _getL1BlockNumber()
        );
    }

    function castVoteWithReason(
        bytes32 _thingProposalId,
        uint16 _proposalVerifiersArrayIndex,
        Vote _vote,
        string calldata _reason
    )
        public
        whenActiveAndNotExpired(_thingProposalId)
        onlyDesignatedVerifier(_thingProposalId, _proposalVerifiersArrayIndex)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        emit CastedVoteWithReason(
            thingId,
            proposalId,
            msg.sender,
            _vote,
            _reason,
            _getL1BlockNumber()
        );
    }

    function getVerifiers(
        bytes32 _thingProposalId
    ) public view returns (address[] memory) {
        return s_proposalVerifiers[_thingProposalId];
    }

    function _unstakeOrUnstakeAndSlashVerifiers(
        bytes32 _thingProposalId,
        uint64[] calldata _verifiersToSlashIndices
    ) private returns (address[] memory slashedVerifiers) {
        uint64 j = 0;
        address[] memory verifiers = s_proposalVerifiers[_thingProposalId];
        slashedVerifiers = new address[](_verifiersToSlashIndices.length);
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
    }

    function finalizePoll__Unsettled(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        Decision _decision,
        uint64[] calldata _verifiersToSlashIndices
    )
        public
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_proposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        address[] memory slashedVerifiers = _unstakeOrUnstakeAndSlashVerifiers(
            _thingProposalId,
            _verifiersToSlashIndices
        );

        emit PollFinalized(
            thingId,
            proposalId,
            _decision,
            _voteAggIpfsCid,
            submitter,
            new address[](0),
            slashedVerifiers
        );
    }

    function _rewardOrSlashVerifiers(
        bytes32 _thingProposalId,
        uint64[] calldata _verifiersToSlashIndices
    )
        private
        returns (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        )
    {
        uint64 j = 0;
        address[] memory verifiers = s_proposalVerifiers[_thingProposalId];
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
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    )
        public
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_proposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndRewardProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingProposalId, _verifiersToSlashIndices);

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function finalizePoll__Declined__Soft(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    )
        public
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_proposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingProposalId, _verifiersToSlashIndices);

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function finalizePoll__Declined__Hard(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        uint64[] calldata _verifiersToSlashIndices
    )
        public
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_proposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndSlashProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardOrSlashVerifiers(_thingProposalId, _verifiersToSlashIndices);

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            rewardedVerifiers,
            slashedVerifiers
        );
    }

    function getUserIndexAmongProposalVerifiers(
        bytes32 _thingProposalId,
        address _user
    ) public view returns (int256) {
        int256 index = -1;
        address[] memory verifiers = s_proposalVerifiers[_thingProposalId];
        for (uint256 i = 0; i < verifiers.length; ++i) {
            if (verifiers[i] == _user) {
                index = int256(i);
                break;
            }
        }

        return index;
    }
}
