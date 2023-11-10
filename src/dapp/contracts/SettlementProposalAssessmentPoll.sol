// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./SettlementProposalAssessmentVerifierLottery.sol";
import "./L1Block.sol";

error SettlementProposalAssessmentPoll__Unauthorized();
error SettlementProposalAssessmentPoll__Expired(bytes32 thingProposalId);
error SettlementProposalAssessmentPoll__NotDesignatedVerifier(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentPoll__NotActive(bytes32 thingProposalId);
error SettlementProposalAssessmentPoll__StillInProgress(
    bytes32 thingProposalId
);

contract SettlementProposalAssessmentPoll {
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
    address private s_settlementProposalAssessmentVerifierLotteryAddress;
    address private s_orchestrator;

    L1Block private constant L1BLOCK =
        L1Block(0x4200000000000000000000000000000000000015);

    uint16 public s_durationBlocks;
    uint8 public s_votingVolumeThresholdPercent;
    uint8 public s_majorityThresholdPercent;

    bytes32[] private s_thingProposals;
    mapping(bytes32 => int256) private s_thingProposalIdToPollInitBlock;
    mapping(bytes32 => address[]) private s_settlementProposalVerifiers;

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
            revert SettlementProposalAssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert SettlementProposalAssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier onlySettlementProposalAssessmentVerifierLottery() {
        if (
            msg.sender != s_settlementProposalAssessmentVerifierLotteryAddress
        ) {
            revert SettlementProposalAssessmentPoll__Unauthorized();
        }
        _;
    }

    modifier whenActiveAndNotExpired(bytes32 _thingProposalId) {
        int256 pollInitBlock = s_thingProposalIdToPollInitBlock[
            _thingProposalId
        ];
        if (pollInitBlock < 1) {
            revert SettlementProposalAssessmentPoll__NotActive(
                _thingProposalId
            );
        }
        if (_getL1BlockNumber() > uint256(pollInitBlock) + s_durationBlocks) {
            revert SettlementProposalAssessmentPoll__Expired(_thingProposalId);
        }
        _;
    }

    modifier onlyDesignatedVerifier(
        bytes32 _thingProposalId,
        uint16 _settlementProposalVerifiersArrayIndex
    ) {
        if (
            s_settlementProposalVerifiers[_thingProposalId][
                _settlementProposalVerifiersArrayIndex
            ] != msg.sender
        ) {
            revert SettlementProposalAssessmentPoll__NotDesignatedVerifier(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenActive(bytes32 _thingProposalId) {
        if (s_thingProposalIdToPollInitBlock[_thingProposalId] < 1) {
            revert SettlementProposalAssessmentPoll__NotActive(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenExpired(bytes32 _thingProposalId) {
        if (
            _getL1BlockNumber() <=
            uint256(s_thingProposalIdToPollInitBlock[_thingProposalId]) +
                s_durationBlocks
        ) {
            revert SettlementProposalAssessmentPoll__StillInProgress(
                _thingProposalId
            );
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

    function _splitIds(
        bytes32 _thingProposalId
    ) private pure returns (bytes16, bytes16) {
        return (
            bytes16(_thingProposalId),
            bytes16(uint128(uint256(_thingProposalId)))
        );
    }

    function setSettlementProposalAssessmentVerifierLotteryAddress(
        address _settlementProposalAssessmentVerifierLotteryAddress
    ) external onlyOrchestrator {
        s_settlementProposalAssessmentVerifierLotteryAddress = _settlementProposalAssessmentVerifierLotteryAddress;
    }

    function exportData()
        external
        view
        returns (
            bytes32[] memory thingProposalIds,
            int256[] memory initBlockNumbers,
            address[][] memory verifiers
        )
    {
        thingProposalIds = s_thingProposals;
        initBlockNumbers = new int256[](thingProposalIds.length);
        verifiers = new address[][](thingProposalIds.length);
        for (uint256 i = 0; i < thingProposalIds.length; ++i) {
            initBlockNumbers[i] = s_thingProposalIdToPollInitBlock[
                thingProposalIds[i]
            ];
            verifiers[i] = s_settlementProposalVerifiers[thingProposalIds[i]];
        }
    }

    function importData(
        bytes32[] calldata _thingProposalIds,
        int256[] calldata _initBlockNumbers,
        address[][] calldata _verifiers
    ) external onlyOrchestrator {
        s_thingProposals = _thingProposalIds;
        for (uint256 i = 0; i < _thingProposalIds.length; ++i) {
            bytes32 thingProposalId = _thingProposalIds[i];
            s_thingProposalIdToPollInitBlock[
                thingProposalId
            ] = _initBlockNumbers[i];
            s_settlementProposalVerifiers[thingProposalId] = _verifiers[i];
        }
    }

    function _getL1BlockNumber() private view returns (uint256) {
        if (block.chainid == 901) {
            return L1BLOCK.number();
        }
        return block.number;
    }

    function getPollInitBlock(
        bytes32 _thingProposalId
    ) external view returns (int256) {
        return s_thingProposalIdToPollInitBlock[_thingProposalId];
    }

    function initPoll(
        bytes32 _thingProposalId,
        address[] memory _verifiers
    ) external onlySettlementProposalAssessmentVerifierLottery {
        s_thingProposals.push(_thingProposalId);
        s_thingProposalIdToPollInitBlock[_thingProposalId] = int256(
            _getL1BlockNumber()
        );
        s_settlementProposalVerifiers[_thingProposalId] = _verifiers;
    }

    function castVote(
        bytes32 _thingProposalId,
        uint16 _settlementProposalVerifiersArrayIndex,
        Vote _vote
    )
        external
        whenActiveAndNotExpired(_thingProposalId)
        onlyDesignatedVerifier(
            _thingProposalId,
            _settlementProposalVerifiersArrayIndex
        )
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
        uint16 _settlementProposalVerifiersArrayIndex,
        Vote _vote,
        string calldata _reason
    )
        external
        whenActiveAndNotExpired(_thingProposalId)
        onlyDesignatedVerifier(
            _thingProposalId,
            _settlementProposalVerifiersArrayIndex
        )
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
    ) external view returns (address[] memory) {
        return s_settlementProposalVerifiers[_thingProposalId];
    }

    function _unstakeOrUnstakeAndSlashVerifiers(
        bytes32 _thingProposalId,
        uint64[] calldata _verifiersToSlashIndices
    ) private returns (address[] memory slashedVerifiers) {
        uint64 j = 0;
        address[] memory verifiers = s_settlementProposalVerifiers[
            _thingProposalId
        ];
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
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_thingProposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_thingProposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeSettlementProposalSubmitter(submitter);

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
        address[] memory verifiers = s_settlementProposalVerifiers[
            _thingProposalId
        ];
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
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_thingProposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_thingProposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndRewardSettlementProposalSubmitter(submitter);

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
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_thingProposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_thingProposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeSettlementProposalSubmitter(submitter);

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
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_thingProposalIdToPollInitBlock[
            _thingProposalId
        ] = -s_thingProposalIdToPollInitBlock[_thingProposalId];
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndSlashSettlementProposalSubmitter(submitter);

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

    function getUserIndexAmongSettlementProposalVerifiers(
        bytes32 _thingProposalId,
        address _user
    ) external view returns (int256) {
        int256 index = -1;
        address[] memory verifiers = s_settlementProposalVerifiers[
            _thingProposalId
        ];
        for (uint256 i = 0; i < verifiers.length; ++i) {
            if (verifiers[i] == _user) {
                index = int256(i);
                break;
            }
        }

        return index;
    }
}
