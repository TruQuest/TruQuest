// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingAssessmentVerifierLottery.sol";

error AssessmentPoll__Unauthorized();
error AssessmentPoll__Expired(bytes32 thingProposalId);
error AssessmentPoll__NotDesignatedVerifier(bytes32 thingProposalId);
error AssessmentPoll__NotInProgress(bytes32 thingProposalId);

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

    enum Stage {
        None,
        InProgress,
        Finalized
    }

    TruQuest private immutable i_truQuest;
    ThingAssessmentVerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    uint16 private s_durationBlocks;
    uint8 private s_votingVolumeThresholdPercent;
    uint8 private s_majorityThresholdPercent;

    mapping(bytes32 => uint64) private s_proposalIdToPollInitBlock;
    mapping(bytes32 => address[]) private s_proposalVerifiers;
    mapping(bytes32 => Stage) private s_proposalPollStage;

    event CastedVote(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        Vote vote
    );

    event CastedVoteWithReason(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        Vote vote,
        string reason
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

    modifier onlyWhileNotExpired(bytes32 _thingProposalId) {
        if (
            block.number >
            s_proposalIdToPollInitBlock[_thingProposalId] + s_durationBlocks
        ) {
            revert AssessmentPoll__Expired(_thingProposalId);
        }
        _;
    }

    modifier onlyDesignatedVerifiers(bytes32 _thingProposalId) {
        uint256 designatedVerifiersCount = s_proposalVerifiers[_thingProposalId]
            .length; // @@??: static var ?
        bool isDesignatedVerifier = false;
        // while?
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (msg.sender == s_proposalVerifiers[_thingProposalId][i]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AssessmentPoll__NotDesignatedVerifier(_thingProposalId);
        }
        _;
    }

    modifier onlyWhenInProgress(bytes32 _thingProposalId) {
        if (s_proposalPollStage[_thingProposalId] != Stage.InProgress) {
            revert AssessmentPoll__NotInProgress(_thingProposalId);
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

    function getPollDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getPollInitBlock(
        bytes32 _thingProposalId
    ) public view returns (uint64) {
        return s_proposalIdToPollInitBlock[_thingProposalId];
    }

    function initPoll(
        bytes32 _thingProposalId,
        address[] memory _verifiers
    ) external onlyThingAssessmentVerifierLottery {
        s_proposalIdToPollInitBlock[_thingProposalId] = uint64(block.number);
        s_proposalVerifiers[_thingProposalId] = _verifiers;
        s_proposalPollStage[_thingProposalId] = Stage.InProgress;
    }

    function checkIsDesignatedVerifierForProposal(
        bytes32 _thingProposalId,
        address _user
    ) public view returns (bool) {
        uint256 designatedVerifiersCount = s_proposalVerifiers[_thingProposalId]
            .length;
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (_user == s_proposalVerifiers[_thingProposalId][i]) {
                return true;
            }
        }

        return false;
    }

    function castVote(
        bytes32 _thingProposalId,
        Vote _vote
    )
        public
        onlyWhenInProgress(_thingProposalId)
        onlyWhileNotExpired(_thingProposalId)
        onlyDesignatedVerifiers(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        emit CastedVote(thingId, proposalId, msg.sender, _vote);
    }

    function castVoteWithReason(
        bytes32 _thingProposalId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenInProgress(_thingProposalId)
        onlyWhileNotExpired(_thingProposalId)
        onlyDesignatedVerifiers(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        emit CastedVoteWithReason(
            thingId,
            proposalId,
            msg.sender,
            _vote,
            _reason
        );
    }

    function getVerifiers(
        bytes32 _thingProposalId
    ) public view returns (address[] memory) {
        return s_proposalVerifiers[_thingProposalId];
    }

    function finalizePoll__Unsettled(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        Decision _decision,
        uint64[] calldata _verifiersToSlashIndices
    ) public onlyOrchestrator onlyWhenInProgress(_thingProposalId) {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        uint64 j = 0;
        address[] memory verifiers = s_proposalVerifiers[_thingProposalId];
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
            thingId,
            proposalId,
            _decision,
            _voteAggIpfsCid,
            submitter,
            new address[](0),
            slashedVerifiers
        );
    }

    function _rewardAndSlashVerifiers(
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
    ) public onlyOrchestrator onlyWhenInProgress(_thingProposalId) {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndRewardProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardAndSlashVerifiers(
                _thingProposalId,
                _verifiersToSlashIndices
            );

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
    ) public onlyOrchestrator onlyWhenInProgress(_thingProposalId) {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardAndSlashVerifiers(
                _thingProposalId,
                _verifiersToSlashIndices
            );

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
    ) public onlyOrchestrator onlyWhenInProgress(_thingProposalId) {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndSlashProposalSubmitter(submitter);

        (
            address[] memory rewardedVerifiers,
            address[] memory slashedVerifiers
        ) = _rewardAndSlashVerifiers(
                _thingProposalId,
                _verifiersToSlashIndices
            );

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
}
