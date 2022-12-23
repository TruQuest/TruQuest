// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingAssessmentVerifierLottery.sol";

error AssessmentPoll__NotOrchestrator();
error AssessmentPoll__NotTruQuest();
error AssessmentPoll__NotVerifierLottery();
error AssessmentPoll__Expired(bytes32 combinedId);
error AssessmentPoll__NotDesignatedVerifier(bytes32 combinedId);
error AssessmentPoll__PollNotInProgress(bytes32 combinedId);
error AssessmentPoll__NotDesignatedSubstituteVerifier(bytes32 combinedId);
error AssessmentPoll__SubPollNotInProgress(bytes32 combinedId);

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

    uint256 private s_proposalAcceptedReward;
    uint256 private s_verifierReward;
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

    modifier onlyWhileNotExpired(bytes32 _combinedId) {
        if (
            block.number >
            s_proposalIdToPollInitBlock[_combinedId] + s_durationBlocks
        ) {
            revert AssessmentPoll__Expired(_combinedId);
        }
        _;
    }

    modifier onlyDesignatedVerifiers(bytes32 _combinedId) {
        uint256 designatedVerifiersCount = s_proposalVerifiers[_combinedId]
            .length; // @@??: static var ?
        bool isDesignatedVerifier = false;
        // while?
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (msg.sender == s_proposalVerifiers[_combinedId][i]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AssessmentPoll__NotDesignatedVerifier(_combinedId);
        }
        _;
    }

    modifier onlyWhenPollInProgress(bytes32 _combinedId) {
        if (s_proposalPollStage[_combinedId] != Stage.InProgress) {
            revert AssessmentPoll__PollNotInProgress(_combinedId);
        }
        _;
    }

    modifier onlyWhenSubPollInProgress(bytes32 _combinedId) {
        if (s_proposalPollStage[_combinedId] != Stage.SubPollInProgress) {
            revert AssessmentPoll__SubPollNotInProgress(_combinedId);
        }
        _;
    }

    modifier onlyDesignatedSubstituteVerifiers(bytes32 _combinedId) {
        int256 designatedVerifiersCount = int256(
            s_proposalVerifiers[_combinedId].length
        );
        bool isDesignatedVerifier;
        // while?
        for (int256 i = designatedVerifiersCount - 1; i > -1; --i) {
            if (msg.sender == s_proposalVerifiers[_combinedId][uint256(i)]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AssessmentPoll__NotDesignatedSubstituteVerifier(_combinedId);
        }
        _;
    }

    modifier onlyWhenPollOrSubPollInProgress(bytes32 _combinedId) {
        Stage stage = s_proposalPollStage[_combinedId];
        if (stage != Stage.InProgress && stage != Stage.SubPollInProgress) {
            revert AssessmentPoll__PollNotInProgress(_combinedId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint256 _proposalAcceptedReward,
        uint256 _verifierReward,
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_proposalAcceptedReward = _proposalAcceptedReward;
        s_verifierReward = _verifierReward;
        s_durationBlocks = _durationBlocks;
    }

    function _combineIds(
        bytes16 _thingId,
        bytes16 _settlementProposalId
    ) private pure returns (bytes32) {
        return
            bytes32(
                (uint256(uint128(_thingId)) << 128) |
                    uint128(_settlementProposalId)
            );
    }

    function _separateIds(
        bytes32 _combinedId
    ) private pure returns (bytes16 _thingId, bytes16 _settlementProposalId) {
        _thingId = bytes16(_combinedId);
        _settlementProposalId = bytes16(uint128(uint256(_combinedId)));
    }

    function connectToThingAssessmentVerifierLottery(
        address _verifierLotteryAddress
    ) external onlyTruQuest {
        s_verifierLottery = ThingAssessmentVerifierLottery(
            _verifierLotteryAddress
        );
    }

    function initPoll(
        bytes16 _thingId,
        bytes16 _settlementProposalId,
        address[] memory _verifiers
    ) external onlyThingAssessmentVerifierLottery {
        bytes32 combinedId = _combineIds(_thingId, _settlementProposalId);
        s_proposalIdToPollInitBlock[combinedId] = uint64(block.number);
        s_proposalVerifiers[combinedId] = _verifiers;
        s_proposalPollStage[combinedId] = Stage.InProgress;
    }

    function castVote(
        bytes32 _combinedId,
        Vote _vote
    )
        public
        onlyWhenPollInProgress(_combinedId)
        onlyWhileNotExpired(_combinedId)
        onlyDesignatedVerifiers(_combinedId)
    {
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        emit CastedVote(thingId, settlementProposalId, msg.sender, _vote);
    }

    function castVoteWithReason(
        bytes32 _combinedId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenPollInProgress(_combinedId)
        onlyWhileNotExpired(_combinedId)
        onlyDesignatedVerifiers(_combinedId)
    {
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        emit CastedVoteWithReason(
            thingId,
            settlementProposalId,
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
        bytes32 _combinedId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToKeep,
        address[] calldata _verifiersToSlash,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyWhenPollInProgress(_combinedId) {
        // array asserts?
        s_proposalPollStage[_combinedId] = Stage.Frozen;

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        s_proposalVerifiers[_combinedId] = _verifiersToKeep;

        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        // s_verifierLottery.initSubLottery(settlementProposalId, _dataHash);

        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);

        emit PollFrozen(
            thingId,
            settlementProposalId,
            Decision.Unsettled__InsufficientVotingVolume,
            _voteAggIpfsCid,
            submitter,
            _verifiersToSlash
        );
    }

    function initSubPoll(
        bytes16 _thingId,
        bytes16 _settlementProposalId,
        address[] memory _substituteVerifiers
    ) external onlyThingAssessmentVerifierLottery {
        bytes32 combinedId = _combineIds(_thingId, _settlementProposalId);
        s_proposalIdToPollInitBlock[combinedId] = uint64(block.number);
        for (uint8 i = 0; i < _substituteVerifiers.length; ++i) {
            s_proposalVerifiers[combinedId].push(_substituteVerifiers[i]);
        }
        s_proposalPollStage[combinedId] = Stage.SubPollInProgress;
    }

    function castVoteAsSubstituteVerifier(
        bytes32 _combinedId,
        Vote _vote
    )
        public
        onlyWhenSubPollInProgress(_combinedId)
        onlyWhileNotExpired(_combinedId)
        onlyDesignatedSubstituteVerifiers(_combinedId)
    {
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        emit CastedVoteAsSubstituteVerifier(
            thingId,
            settlementProposalId,
            msg.sender,
            _vote
        );
    }

    function castVoteWithReasonAsSubstituteVerifier(
        bytes32 _combinedId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenSubPollInProgress(_combinedId)
        onlyWhileNotExpired(_combinedId)
        onlyDesignatedSubstituteVerifiers(_combinedId)
    {
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        emit CastedVoteWithReasonAsSubstituteVerifier(
            thingId,
            settlementProposalId,
            msg.sender,
            _vote,
            _reason
        );
    }

    function finalizePoll__Accepted(
        bytes32 _combinedId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_combinedId) {
        s_proposalPollStage[_combinedId] = Stage.Finalized;
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeAndReward(submitter, s_proposalAcceptedReward);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        emit PollFinalized(
            thingId,
            settlementProposalId,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Soft(
        bytes32 _combinedId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_combinedId) {
        s_proposalPollStage[_combinedId] = Stage.Finalized;

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );

        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);

        emit PollFinalized(
            thingId,
            settlementProposalId,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Hard(
        bytes32 _combinedId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_combinedId) {
        s_proposalPollStage[_combinedId] = Stage.Finalized;
        (bytes16 thingId, bytes16 settlementProposalId) = _separateIds(
            _combinedId
        );
        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.slash(submitter, 0); // amount?

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        emit PollFinalized(
            thingId,
            settlementProposalId,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function getVerifierCount(
        bytes16 _thingId,
        bytes16 _settlementProposalId
    ) external view returns (uint256) {
        bytes32 combinedId = _combineIds(_thingId, _settlementProposalId);
        return s_proposalVerifiers[combinedId].length;
    }
}
