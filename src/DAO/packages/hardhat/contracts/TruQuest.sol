// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TRU.sol";
import "./VerifierLottery.sol";

error TruQuest__ThingAlreadyFunded(string thingId);
error TruQuest__NotEnoughFunds(uint256 requiredAmount, uint256 availableAmount);
error TruQuest__NotOrchestrator();
error TruQuest__NotVerifierLottery();
error TruQuest__ThingAcceptancePollExpired(string thingId);
error TruQuest__NotDesignatedVerifier(string thingId);
error TruQuest__AcceptancePollNotInProgress(string thingId);
error TruQuest__NotDesignatedSubstituteVerifier(string thingId);
error TruQuest__AcceptanceSubPollNotInProgress(string thingId);

contract TruQuest {
    enum AcceptanceVote {
        Decline__Soft,
        Decline__Hard,
        Accept
    }

    enum AcceptancePollDecision {
        Unsettled__InsufficientVotingVolume,
        Unsettled__MajorityThresholdNotReached,
        Declined__Soft,
        Declined__Hard,
        Accepted
    }

    enum ThingLifecycleStage {
        Initial,
        AcceptancePoll__InProgress,
        AcceptancePoll__Frozen,
        AcceptancePoll__SubPollInProgress,
        AcceptancePoll__Finalized
    }

    TRU private immutable i_truToken;
    VerifierLottery public immutable i_verifierLottery;
    address private s_orchestrator;

    uint8 private s_numVerifiers;
    uint256 private s_verifierStake;
    uint256 private s_thingStake;
    uint256 private s_thingSubmissionAcceptedReward;
    uint256 private s_verifierReward;
    uint16 private s_acceptancePollDurationBlocks;

    mapping(address => uint256) private s_balanceOf;
    mapping(address => uint256) private s_stakedBalanceOf;

    // @@TODO: Join some of these into one mapping.
    mapping(string => address) private s_thingSubmitter;
    mapping(string => uint64) private s_thingIdToVerifiersSelectedBlock;
    mapping(string => address[]) public s_thingVerifiers;
    mapping(string => ThingLifecycleStage) private s_thingLifecycleStage;

    event FundsDeposited(address indexed user, uint256 amount);
    event ThingFunded(
        string indexed thingId,
        address indexed user,
        uint256 thingStake
    );
    event CastedThingAcceptanceVote(
        string indexed thingId,
        address indexed user,
        AcceptanceVote vote
    );
    event CastedThingAcceptanceVoteWithReason(
        string indexed thingId,
        address indexed user,
        AcceptanceVote vote,
        string reason
    );
    event AcceptancePollFrozen(
        string indexed thingId,
        address orchestrator,
        AcceptancePollDecision decision,
        string voteAggIpfsUri,
        address submitter,
        address[] slashedVerifiers
    );
    event CastedThingAcceptanceVoteAsSubstituteVerifier(
        string indexed thingId,
        address indexed user,
        AcceptanceVote vote
    );
    event CastedThingAcceptanceVoteWithReasonAsSubstituteVerifier(
        string indexed thingId,
        address indexed user,
        AcceptanceVote vote,
        string reason
    );
    event AcceptancePollFinalized(
        string indexed thingId,
        address orchestrator,
        AcceptancePollDecision decision,
        string voteAggIpfsUri,
        address submitter,
        address[] rewardedVerifiers,
        address[] slashedVerifiers
    );

    modifier whenHasAtLeast(uint256 _requiredFunds) {
        uint256 availableFunds = getAvailableFunds(msg.sender);
        if (availableFunds < _requiredFunds) {
            revert TruQuest__NotEnoughFunds(_requiredFunds, availableFunds);
        }
        _;
    }

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert TruQuest__NotOrchestrator();
        }
        _;
    }

    modifier onlyVerifierLottery() {
        if (msg.sender != address(i_verifierLottery)) {
            revert TruQuest__NotVerifierLottery();
        }
        _;
    }

    modifier onlyWhenNotFunded(string calldata _thingId) {
        if (s_thingSubmitter[_thingId] != address(0)) {
            revert TruQuest__ThingAlreadyFunded(_thingId);
        }
        _;
    }

    modifier onlyWhileAcceptancePollNotExpired(string calldata _thingId) {
        if (
            block.number >
            s_thingIdToVerifiersSelectedBlock[_thingId] +
                s_acceptancePollDurationBlocks
        ) {
            revert TruQuest__ThingAcceptancePollExpired(_thingId);
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
            revert TruQuest__NotDesignatedVerifier(_thingId);
        }
        _;
    }

    modifier onlyWhenAcceptancePollInProgress(string calldata _thingId) {
        if (
            s_thingLifecycleStage[_thingId] !=
            ThingLifecycleStage.AcceptancePoll__InProgress
        ) {
            revert TruQuest__AcceptancePollNotInProgress(_thingId);
        }
        _;
    }

    modifier onlyWhenAcceptanceSubPollInProgress(string calldata _thingId) {
        if (
            s_thingLifecycleStage[_thingId] !=
            ThingLifecycleStage.AcceptancePoll__SubPollInProgress
        ) {
            revert TruQuest__AcceptanceSubPollNotInProgress(_thingId);
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
            revert TruQuest__NotDesignatedSubstituteVerifier(_thingId);
        }
        _;
    }

    modifier onlyWhenAcceptancePollOrSubPollInProgress(
        string calldata _thingId
    ) {
        ThingLifecycleStage stage = s_thingLifecycleStage[_thingId];
        if (
            stage != ThingLifecycleStage.AcceptancePoll__InProgress &&
            stage != ThingLifecycleStage.AcceptancePoll__SubPollInProgress
        ) {
            revert TruQuest__AcceptancePollNotInProgress(_thingId);
        }
        _;
    }

    constructor(
        address _truTokenAddress,
        uint8 _numVerifiers,
        uint256 _verifierStake,
        uint256 _thingStake,
        uint256 _thingSubmissionAcceptedReward,
        uint256 _verifierReward,
        uint16 _verifierLotteryDurationBlocks,
        uint16 _acceptancePollDurationBlocks
    ) {
        i_truToken = TRU(_truTokenAddress);
        i_verifierLottery = new VerifierLottery(
            address(this),
            _numVerifiers,
            _verifierStake,
            _verifierLotteryDurationBlocks
        );
        s_orchestrator = msg.sender;
        s_numVerifiers = _numVerifiers;
        s_verifierStake = _verifierStake;
        s_thingStake = _thingStake;
        s_thingSubmissionAcceptedReward = _thingSubmissionAcceptedReward;
        s_verifierReward = _verifierReward;
        s_acceptancePollDurationBlocks = _acceptancePollDurationBlocks;
    }

    function deposit(uint256 _amount) public {
        i_truToken.transferFrom(msg.sender, address(this), _amount);
        s_balanceOf[msg.sender] += _amount;
        emit FundsDeposited(msg.sender, _amount);
    }

    function stake(address _user, uint256 _amount)
        external
        onlyVerifierLottery
    {
        s_stakedBalanceOf[_user] += _amount;
    }

    function unstake(address _user, uint256 _amount)
        external
        onlyVerifierLottery
    {
        s_stakedBalanceOf[_user] -= _amount;
    }

    function _stake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] += _amount;
    }

    function _unstake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] -= _amount;
    }

    function _slash(address _user, uint256 _amount) private {
        s_balanceOf[_user] -= _amount;
        s_stakedBalanceOf[_user] -= _amount;
    }

    function _reward(address _user, uint256 _amount) private {
        // i_truToken.transfer(_user, _amount);
        s_balanceOf[_user] += _amount;
    }

    function getAvailableFunds(address _user) public view returns (uint256) {
        return s_balanceOf[_user] - s_stakedBalanceOf[_user];
    }

    function checkHasAtLeast(address _user, uint256 _requiredFunds)
        external
        view
        returns (bool)
    {
        return getAvailableFunds(_user) >= _requiredFunds;
    }

    // Is there a risk of user A submitting a thing, but then user B funding (stealing) it before A can?
    function fundThing(string calldata _thingId)
        public
        onlyWhenNotFunded(_thingId)
        whenHasAtLeast(s_thingStake)
    {
        s_thingSubmitter[_thingId] = msg.sender;
        _stake(msg.sender, s_thingStake);
        emit ThingFunded(_thingId, msg.sender, s_thingStake);
    }

    function initAcceptancePoll(
        string calldata _thingId,
        address[] memory _verifiers
    ) external onlyVerifierLottery {
        s_thingIdToVerifiersSelectedBlock[_thingId] = uint64(block.number);
        s_thingVerifiers[_thingId] = _verifiers;
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__InProgress;
    }

    function castAcceptanceVote(string calldata _thingId, AcceptanceVote _vote)
        public
        onlyWhenAcceptancePollInProgress(_thingId)
        onlyWhileAcceptancePollNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedThingAcceptanceVote(_thingId, msg.sender, _vote);
    }

    function castAcceptanceVoteWithReason(
        string calldata _thingId,
        AcceptanceVote _vote,
        string calldata _reason
    )
        public
        onlyWhenAcceptancePollInProgress(_thingId)
        onlyWhileAcceptancePollNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedThingAcceptanceVoteWithReason(
            _thingId,
            msg.sender,
            _vote,
            _reason
        );
    }

    //   function freezeAcceptancePoll__Unsettled__MajorityThresholdNotReached(string calldata _thingId, string calldata _voteAggIpfsUri)
    //     public
    //     onlyOrchestrator
    //     onlyWhenAcceptancePollInProgress(_thingId)
    //   {
    //     s_thingLifecycleStage[_thingId] = ThingLifecycleStage.AcceptancePoll__Frozen;
    //     emit AcceptancePollFrozen(
    //       _thingId,
    //       s_orchestrator,
    //       AcceptancePollDecision.Unsettled__MajorityThresholdNotReached,
    //       _voteAggIpfsUri,
    //       s_thingSubmitter[_thingId],
    //       new address[](0)
    //     );
    //   }

    function freezeAcceptancePoll__Unsettled__InsufficientVotingVolume(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
        address[] calldata _verifiersToKeep,
        address[] calldata _verifiersToSlash,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyWhenAcceptancePollInProgress(_thingId) {
        // array asserts?
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__Frozen;

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            _slash(_verifiersToSlash[i], s_verifierStake); // amount?
        }

        s_thingVerifiers[_thingId] = _verifiersToKeep;
        i_verifierLottery.initVerifierSubLottery(_thingId, _dataHash);

        address submitter = s_thingSubmitter[_thingId];

        emit AcceptancePollFrozen(
            _thingId,
            s_orchestrator,
            AcceptancePollDecision.Unsettled__InsufficientVotingVolume,
            _voteAggIpfsUri,
            submitter,
            _verifiersToSlash
        );
    }

    function initAcceptanceSubPoll(
        string calldata _thingId,
        address[] memory _substituteVerifiers
    ) external onlyVerifierLottery {
        s_thingIdToVerifiersSelectedBlock[_thingId] = uint64(block.number);
        for (uint8 i = 0; i < _substituteVerifiers.length; ++i) {
            s_thingVerifiers[_thingId].push(_substituteVerifiers[i]);
        }
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__SubPollInProgress;
    }

    function castAcceptanceVoteAsSubstituteVerifier(
        string calldata _thingId,
        AcceptanceVote _vote
    )
        public
        onlyWhenAcceptanceSubPollInProgress(_thingId)
        onlyWhileAcceptancePollNotExpired(_thingId)
        onlyDesignatedSubstituteVerifiers(_thingId)
    {
        emit CastedThingAcceptanceVoteAsSubstituteVerifier(
            _thingId,
            msg.sender,
            _vote
        );
    }

    function castAcceptanceVoteWithReasonAsSubstituteVerifier(
        string calldata _thingId,
        AcceptanceVote _vote,
        string calldata _reason
    )
        public
        onlyWhenAcceptanceSubPollInProgress(_thingId)
        onlyWhileAcceptancePollNotExpired(_thingId)
        onlyDesignatedSubstituteVerifiers(_thingId)
    {
        emit CastedThingAcceptanceVoteWithReasonAsSubstituteVerifier(
            _thingId,
            msg.sender,
            _vote,
            _reason
        );
    }

    function finalizeAcceptancePoll__Accepted(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenAcceptancePollOrSubPollInProgress(_thingId)
    {
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__Finalized;
        address submitter = s_thingSubmitter[_thingId];
        _reward(submitter, s_thingSubmissionAcceptedReward);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            _reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            _slash(_verifiersToSlash[i], s_verifierStake); // amount?
        }

        emit AcceptancePollFinalized(
            _thingId,
            s_orchestrator,
            AcceptancePollDecision.Accepted,
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizeAcceptancePoll__Declined__Soft(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenAcceptancePollOrSubPollInProgress(_thingId)
    {
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__Finalized;

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            _reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            _slash(_verifiersToSlash[i], s_verifierStake); // amount?
        }

        address submitter = s_thingSubmitter[_thingId];

        emit AcceptancePollFinalized(
            _thingId,
            s_orchestrator,
            AcceptancePollDecision.Declined__Soft,
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizeAcceptancePoll__Declined__Hard(
        string calldata _thingId,
        string calldata _voteAggIpfsUri,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    )
        public
        onlyOrchestrator
        onlyWhenAcceptancePollOrSubPollInProgress(_thingId)
    {
        s_thingLifecycleStage[_thingId] = ThingLifecycleStage
            .AcceptancePoll__Finalized;
        address submitter = s_thingSubmitter[_thingId];
        _slash(submitter, s_thingStake); // amount?

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            _reward(_verifiersToReward[i], s_verifierReward);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            _slash(_verifiersToSlash[i], s_verifierStake); // amount?
        }

        emit AcceptancePollFinalized(
            _thingId,
            s_orchestrator,
            AcceptancePollDecision.Declined__Hard,
            _voteAggIpfsUri,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }
}
