// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./VerifierLottery.sol";

error AcceptancePoll__NotOrchestrator();
error AcceptancePoll__NotTruQuest();
error AcceptancePoll__NotVerifierLottery();
error AcceptancePoll__Expired(string thingId);
error AcceptancePoll__NotDesignatedVerifier(string thingId);
error AcceptancePoll__PollNotInProgress(string thingId);
error AcceptancePoll__NotDesignatedSubstituteVerifier(string thingId);
error AcceptancePoll__SubPollNotInProgress(string thingId);

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
    VerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    uint256 private s_thingSubmissionAcceptedReward;
    uint256 private s_verifierReward;
    uint16 private s_durationBlocks;

    mapping(string => uint64) private s_thingIdToPollStartedBlock;
    mapping(string => address[]) private s_thingVerifiers;
    mapping(string => Stage) private s_thingPollStage;

    event CastedVote(string indexed thingId, address indexed user, Vote vote);
    event CastedVoteWithReason(
        string indexed thingId,
        address indexed user,
        Vote vote,
        string reason
    );
    event PollFrozen(
        string indexed thingId,
        address orchestrator,
        Decision decision,
        string voteAggIpfsUri,
        address submitter,
        address[] slashedVerifiers
    );
    event CastedVoteAsSubstituteVerifier(
        string indexed thingId,
        address indexed user,
        Vote vote
    );
    event CastedVoteWithReasonAsSubstituteVerifier(
        string indexed thingId,
        address indexed user,
        Vote vote,
        string reason
    );
    event PollFinalized(
        string indexed thingId,
        address orchestrator,
        Decision decision,
        string voteAggIpfsUri,
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

    modifier onlyVerifierLottery() {
        if (msg.sender != address(s_verifierLottery)) {
            revert AcceptancePoll__NotVerifierLottery();
        }
        _;
    }

    modifier onlyWhileNotExpired(string calldata _thingId) {
        if (
            block.number >
            s_thingIdToPollStartedBlock[_thingId] + s_durationBlocks
        ) {
            revert AcceptancePoll__Expired(_thingId);
        }
        _;
    }

    modifier onlyDesignatedVerifiers(string calldata _thingId) {
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

    modifier onlyWhenPollInProgress(string calldata _thingId) {
        if (s_thingPollStage[_thingId] != Stage.InProgress) {
            revert AcceptancePoll__PollNotInProgress(_thingId);
        }
        _;
    }

    modifier onlyWhenSubPollInProgress(string calldata _thingId) {
        if (s_thingPollStage[_thingId] != Stage.SubPollInProgress) {
            revert AcceptancePoll__SubPollNotInProgress(_thingId);
        }
        _;
    }

    modifier onlyDesignatedSubstituteVerifiers(string calldata _thingId) {
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

    modifier onlyWhenPollOrSubPollInProgress(string calldata _thingId) {
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

    function connectToVerifierLottery(address _verifierLotteryAddress)
        external
        onlyTruQuest
    {
        s_verifierLottery = VerifierLottery(_verifierLotteryAddress);
    }

    function initPoll(string calldata _thingId, address[] memory _verifiers)
        external
        onlyVerifierLottery
    {
        s_thingIdToPollStartedBlock[_thingId] = uint64(block.number); // should be calculated instead of being dependant on backend's timing
        s_thingVerifiers[_thingId] = _verifiers;
        s_thingPollStage[_thingId] = Stage.InProgress;
    }

    function castVote(string calldata _thingId, Vote _vote)
        public
        onlyWhenPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedVote(_thingId, msg.sender, _vote);
    }

    function castVoteWithReason(
        string calldata _thingId,
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

    //   function freezePoll__Unsettled__MajorityThresholdNotReached(string calldata _thingId, string calldata _voteAggIpfsUri)
    //     public
    //     onlyOrchestrator
    //     onlyWhenPollInProgress(_thingId)
    //   {
    //     s_thingPollStage[_thingId] = Stage.Frozen;
    //     emit PollFrozen(
    //       _thingId,
    //       s_orchestrator,
    //       Decision.Unsettled__MajorityThresholdNotReached,
    //       _voteAggIpfsUri,
    //       i_truQuest.s_thingSubmitter(_thingId),
    //       new address[](0)
    //     );
    //   }

    function freezePoll__Unsettled__InsufficientVotingVolume(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
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
            _voteAggIpfsUri,
            submitter,
            _verifiersToSlash
        );
    }

    function initSubPoll(
        string calldata _thingId,
        address[] memory _substituteVerifiers
    ) external onlyVerifierLottery {
        s_thingIdToPollStartedBlock[_thingId] = uint64(block.number); // same as in initPoll
        for (uint8 i = 0; i < _substituteVerifiers.length; ++i) {
            s_thingVerifiers[_thingId].push(_substituteVerifiers[i]);
        }
        s_thingPollStage[_thingId] = Stage.SubPollInProgress;
    }

    function castVoteAsSubstituteVerifier(string calldata _thingId, Vote _vote)
        public
        onlyWhenSubPollInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedSubstituteVerifiers(_thingId)
    {
        emit CastedVoteAsSubstituteVerifier(_thingId, msg.sender, _vote);
    }

    function castVoteWithReasonAsSubstituteVerifier(
        string calldata _thingId,
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
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenPollOrSubPollInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.reward(submitter, s_thingSubmissionAcceptedReward);

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
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Soft(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
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
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Hard(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
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
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function getVerifierCount(string calldata _thingId)
        external
        view
        returns (uint256)
    {
        return s_thingVerifiers[_thingId].length;
    }
}
