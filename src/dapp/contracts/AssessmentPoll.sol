// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingAssessmentVerifierLottery.sol";

error AssessmentPoll__NotOrchestrator();
error AssessmentPoll__NotTruQuest();
error AssessmentPoll__NotVerifierLottery();
error AssessmentPoll__Expired(bytes32 thingProposalId);
error AssessmentPoll__NotDesignatedVerifier(bytes32 thingProposalId);
error AssessmentPoll__PollNotInProgress(bytes32 thingProposalId);
error AssessmentPoll__NotDesignatedSubstituteVerifier(bytes32 thingProposalId);
error AssessmentPoll__SubPollNotInProgress(bytes32 thingProposalId);

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
        Frozen,
        SubPollInProgress,
        Finalized
    }

    TruQuest private immutable i_truQuest;
    ThingAssessmentVerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    uint16 private s_durationBlocks;

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

    event PollFrozen(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] slashedVerifiers
    );

    event CastedVoteAsSubstituteVerifier(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        Vote vote
    );

    event CastedVoteWithReasonAsSubstituteVerifier(
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
            revert AssessmentPoll__NotOrchestrator();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert AssessmentPoll__NotTruQuest();
        }
        _;
    }

    modifier onlyThingAssessmentVerifierLottery() {
        if (msg.sender != address(s_verifierLottery)) {
            revert AssessmentPoll__NotVerifierLottery();
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

    modifier onlyWhenPollInProgress(bytes32 _thingProposalId) {
        if (s_proposalPollStage[_thingProposalId] != Stage.InProgress) {
            revert AssessmentPoll__PollNotInProgress(_thingProposalId);
        }
        _;
    }

    modifier onlyWhenSubPollInProgress(bytes32 _thingProposalId) {
        if (s_proposalPollStage[_thingProposalId] != Stage.SubPollInProgress) {
            revert AssessmentPoll__SubPollNotInProgress(_thingProposalId);
        }
        _;
    }

    modifier onlyDesignatedSubstituteVerifiers(bytes32 _thingProposalId) {
        int256 designatedVerifiersCount = int256(
            s_proposalVerifiers[_thingProposalId].length
        );
        bool isDesignatedVerifier;
        // while?
        for (int256 i = designatedVerifiersCount - 1; i > -1; --i) {
            if (
                msg.sender == s_proposalVerifiers[_thingProposalId][uint256(i)]
            ) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AssessmentPoll__NotDesignatedSubstituteVerifier(
                _thingProposalId
            );
        }
        _;
    }

    modifier onlyWhenPollOrSubPollInProgress(bytes32 _thingProposalId) {
        Stage stage = s_proposalPollStage[_thingProposalId];
        if (stage != Stage.InProgress && stage != Stage.SubPollInProgress) {
            revert AssessmentPoll__PollNotInProgress(_thingProposalId);
        }
        _;
    }

    constructor(address _truQuestAddress, uint16 _durationBlocks) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_durationBlocks = _durationBlocks;
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
        onlyWhenPollInProgress(_thingProposalId)
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
        onlyWhenPollInProgress(_thingProposalId)
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

    //   function freezePoll__Unsettled__MajorityThresholdNotReached(bytes16 _settlementProposalId, string calldata _voteAggIpfsCid)
    //     public
    //     onlyOrchestrator
    //     onlyWhenPollInProgress(_settlementProposalId)
    //   {
    //     s_proposalPollStage[_settlementProposalId] = Stage.Frozen;
    //     emit PollFrozen(
    //       _settlementProposalId,
    //       Decision.Unsettled__MajorityThresholdNotReached,
    //       _voteAggIpfsCid,
    //       i_truQuest.s_thingSubmitter(_settlementProposalId),
    //       new address[](0)
    //     );
    //   }

    function freezePoll__Unsettled__InsufficientVotingVolume(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToKeep,
        address[] calldata _verifiersToSlash,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyWhenPollInProgress(_thingProposalId) {
        // array asserts?
        s_proposalPollStage[_thingProposalId] = Stage.Frozen;

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        s_proposalVerifiers[_thingProposalId] = _verifiersToKeep;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        // s_verifierLottery.initSubLottery(settlementProposalId, _dataHash);

        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);

        emit PollFrozen(
            thingId,
            proposalId,
            Decision.Unsettled__InsufficientVotingVolume,
            _voteAggIpfsCid,
            submitter,
            _verifiersToSlash
        );
    }

    // function initSubPoll(
    //     bytes16 _thingId,
    //     bytes16 _settlementProposalId,
    //     address[] memory _substituteVerifiers
    // ) external onlyThingAssessmentVerifierLottery {
    //     bytes32 combinedId = _combineIds(_thingId, _settlementProposalId);
    //     s_pollInitBlock[combinedId] = uint64(block.number);
    //     for (uint8 i = 0; i < _substituteVerifiers.length; ++i) {
    //         s_verifiers[combinedId].push(_substituteVerifiers[i]);
    //     }
    //     s_pollStage[combinedId] = Stage.SubPollInProgress;
    // }

    // function castVoteAsSubstituteVerifier(
    //     bytes32 _combinedId,
    //     Vote _vote
    // )
    //     public
    //     onlyWhenSubPollInProgress(_combinedId)
    //     onlyWhileNotExpired(_combinedId)
    //     onlyDesignatedSubstituteVerifiers(_combinedId)
    // {
    //     (bytes16 thingId, bytes16 settlementProposalId) = _splitIds(
    //         _combinedId
    //     );
    //     emit CastedVoteAsSubstituteVerifier(
    //         thingId,
    //         settlementProposalId,
    //         msg.sender,
    //         _vote
    //     );
    // }

    // function castVoteWithReasonAsSubstituteVerifier(
    //     bytes32 _combinedId,
    //     Vote _vote,
    //     string calldata _reason
    // )
    //     public
    //     onlyWhenSubPollInProgress(_combinedId)
    //     onlyWhileNotExpired(_combinedId)
    //     onlyDesignatedSubstituteVerifiers(_combinedId)
    // {
    //     (bytes16 thingId, bytes16 settlementProposalId) = _splitIds(
    //         _combinedId
    //     );
    //     emit CastedVoteWithReasonAsSubstituteVerifier(
    //         thingId,
    //         settlementProposalId,
    //         msg.sender,
    //         _vote,
    //         _reason
    //     );
    // }

    function finalizePoll__Accepted(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenPollOrSubPollInProgress(_thingProposalId)
    {
        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndRewardProposalSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Soft(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenPollOrSubPollInProgress(_thingProposalId)
    {
        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Hard(
        bytes32 _thingProposalId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenPollOrSubPollInProgress(_thingProposalId)
    {
        s_proposalPollStage[_thingProposalId] = Stage.Finalized;
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndSlashProposalSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            thingId,
            proposalId,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function getVerifierCount(
        bytes32 _thingProposalId
    ) external view returns (uint256) {
        return s_proposalVerifiers[_thingProposalId].length;
    }
}
