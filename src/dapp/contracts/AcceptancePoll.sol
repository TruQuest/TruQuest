// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingSubmissionVerifierLottery.sol";

error AcceptancePoll__NotOrchestrator();
error AcceptancePoll__NotTruQuest();
error AcceptancePoll__NotVerifierLottery();
error AcceptancePoll__Expired(bytes16 thingId);
error AcceptancePoll__NotDesignatedVerifier(bytes16 thingId);
error AcceptancePoll__PollNotInProgress(bytes16 thingId);
error AcceptancePoll__NotDesignatedSubstituteVerifier(bytes16 thingId);
error AcceptancePoll__SubPollNotInProgress(bytes16 thingId);

contract AcceptancePoll {
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
    ThingSubmissionVerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    uint256 private s_thingSubmissionAcceptedReward;
    uint256 private s_verifierReward;
    uint16 private s_durationBlocks;

    mapping(bytes16 => uint64) private s_thingIdToPollStartedBlock;
    mapping(bytes16 => address[]) private s_thingVerifiers;
    mapping(bytes16 => Stage) private s_thingPollStage;

    event CastedVote(bytes16 indexed thingId, address indexed user, Vote vote);

    event CastedVoteWithReason(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote,
        string reason
    );

    event PollFrozen(
        bytes16 indexed thingId,
        address orchestrator,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] slashedVerifiers
    );

    event CastedVoteAsSubstituteVerifier(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote
    );

    event CastedVoteWithReasonAsSubstituteVerifier(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote,
        string reason
    );

    event PollFinalized(
        bytes16 indexed thingId,
        address orchestrator,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] rewardedVerifiers,
        address[] slashedVerifiers
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert AcceptancePoll__NotOrchestrator();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert AcceptancePoll__NotTruQuest();
        }
        _;
    }

    modifier onlyThingSubmissionVerifierLottery() {
        if (msg.sender != address(s_verifierLottery)) {
            revert AcceptancePoll__NotVerifierLottery();
        }
        _;
    }

    modifier onlyWhileNotExpired(bytes16 _thingId) {
        if (
            block.number >
            s_thingIdToPollStartedBlock[_thingId] + s_durationBlocks
        ) {
            revert AcceptancePoll__Expired(_thingId);
        }
        _;
    }

    modifier onlyDesignatedVerifiers(bytes16 _thingId) {
        uint256 designatedVerifiersCount = s_thingVerifiers[_thingId].length; // @@??: static var ?
        bool isDesignatedVerifier = false;
        // while?
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (msg.sender == s_thingVerifiers[_thingId][i]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AcceptancePoll__NotDesignatedVerifier(_thingId);
        }
        _;
    }

    modifier onlyWhenPollInProgress(bytes16 _thingId) {
        if (s_thingPollStage[_thingId] != Stage.InProgress) {
            revert AcceptancePoll__PollNotInProgress(_thingId);
        }
        _;
    }

    modifier onlyWhenSubPollInProgress(bytes16 _thingId) {
        if (s_thingPollStage[_thingId] != Stage.SubPollInProgress) {
            revert AcceptancePoll__SubPollNotInProgress(_thingId);
        }
        _;
    }

    modifier onlyDesignatedSubstituteVerifiers(bytes16 _thingId) {
        int256 designatedVerifiersCount = int256(
            s_thingVerifiers[_thingId].length
        );
        bool isDesignatedVerifier;
        // while?
        for (int256 i = designatedVerifiersCount - 1; i > -1; --i) {
            if (msg.sender == s_thingVerifiers[_thingId][uint256(i)]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AcceptancePoll__NotDesignatedSubstituteVerifier(_thingId);
        }
        _;
    }

    modifier onlyWhenPollOrSubPollInProgress(bytes16 _thingId) {
        Stage stage = s_thingPollStage[_thingId];
        if (stage != Stage.InProgress && stage != Stage.SubPollInProgress) {
            revert AcceptancePoll__PollNotInProgress(_thingId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint256 _thingSubmissionAcceptedReward,
        uint256 _verifierReward,
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_thingSubmissionAcceptedReward = _thingSubmissionAcceptedReward;
        s_verifierReward = _verifierReward;
        s_durationBlocks = _durationBlocks;
    }

    function connectToThingSubmissionVerifierLottery(
        address _verifierLotteryAddress
    ) external onlyTruQuest {
        s_verifierLottery = ThingSubmissionVerifierLottery(
            _verifierLotteryAddress
        );
    }

    function initPoll(
        bytes16 _thingId,
        address[] memory _verifiers
    ) external onlyThingSubmissionVerifierLottery {
        s_thingIdToPollStartedBlock[_thingId] = uint64(block.number);
        s_thingVerifiers[_thingId] = _verifiers;
        s_thingPollStage[_thingId] = Stage.InProgress;
    }

    function castVote(
        bytes16 _thingId,
        Vote _vote
    )
        public
        onlyWhenPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedVote(_thingId, msg.sender, _vote);
    }

    function castVoteWithReason(
        bytes16 _thingId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedVoteWithReason(_thingId, msg.sender, _vote, _reason);
    }

    //   function freezePoll__Unsettled__MajorityThresholdNotReached(bytes16 _thingId, string calldata _voteAggIpfsCid)
    //     public
    //     onlyOrchestrator
    //     onlyWhenPollInProgress(_thingId)
    //   {
    //     s_thingPollStage[_thingId] = Stage.Frozen;
    //     emit PollFrozen(
    //       _thingId,
    //       s_orchestrator,
    //       Decision.Unsettled__MajorityThresholdNotReached,
    //       _voteAggIpfsCid,
    //       i_truQuest.s_thingSubmitter(_thingId),
    //       new address[](0)
    //     );
    //   }

    function freezePoll__Unsettled__InsufficientVotingVolume(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToKeep,
        address[] calldata _verifiersToSlash,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyWhenPollInProgress(_thingId) {
        // array asserts?
        s_thingPollStage[_thingId] = Stage.Frozen;

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        s_thingVerifiers[_thingId] = _verifiersToKeep;
        s_verifierLottery.initSubLottery(_thingId, _dataHash);

        address submitter = i_truQuest.s_thingSubmitter(_thingId);

        emit PollFrozen(
            _thingId,
            s_orchestrator,
            Decision.Unsettled__InsufficientVotingVolume,
            _voteAggIpfsCid,
            submitter,
            _verifiersToSlash
        );
    }

    function initSubPoll(
        bytes16 _thingId,
        address[] memory _substituteVerifiers
    ) external onlyThingSubmissionVerifierLottery {
        s_thingIdToPollStartedBlock[_thingId] = uint64(block.number); // same as in initPoll
        for (uint8 i = 0; i < _substituteVerifiers.length; ++i) {
            s_thingVerifiers[_thingId].push(_substituteVerifiers[i]);
        }
        s_thingPollStage[_thingId] = Stage.SubPollInProgress;
    }

    function castVoteAsSubstituteVerifier(
        bytes16 _thingId,
        Vote _vote
    )
        public
        onlyWhenSubPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedSubstituteVerifiers(_thingId)
    {
        emit CastedVoteAsSubstituteVerifier(_thingId, msg.sender, _vote);
    }

    function castVoteWithReasonAsSubstituteVerifier(
        bytes16 _thingId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenSubPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedSubstituteVerifiers(_thingId)
    {
        emit CastedVoteWithReasonAsSubstituteVerifier(
            _thingId,
            msg.sender,
            _vote,
            _reason
        );
    }

    function finalizePoll__Accepted(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeAndReward(submitter, s_thingSubmissionAcceptedReward);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        emit PollFinalized(
            _thingId,
            s_orchestrator,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Soft(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        address submitter = i_truQuest.s_thingSubmitter(_thingId);

        emit PollFinalized(
            _thingId,
            s_orchestrator,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Hard(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.slash(submitter, 0); // amount?

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.slash(_verifiersToSlash[i], 0); // amount?
        }

        emit PollFinalized(
            _thingId,
            s_orchestrator,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function getVerifierCount(
        bytes16 _thingId
    ) external view returns (uint256) {
        return s_thingVerifiers[_thingId].length;
    }
}
