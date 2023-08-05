// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;
pragma abicoder v2;

import "@ganache/console.log/console.sol";

import "./Truthserum.sol";
import "./ThingSubmissionVerifierLottery.sol";
import "./AcceptancePoll.sol";
import "./ThingAssessmentVerifierLottery.sol";
import "./AssessmentPoll.sol";

error TruQuest__ThingAlreadyFunded(bytes16 thingId);
error TruQuest__NotEnoughFunds(uint256 requiredAmount, uint256 availableAmount);
error TruQuest__Unauthorized();
error TruQuest__InvalidSignature();
error TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
    bytes16 thingId
);

contract TruQuest {
    struct ThingTd {
        bytes16 id;
    }

    struct SettlementProposalTd {
        bytes16 thingId;
        bytes16 id;
    }

    struct SettlementProposal {
        bytes16 id;
        address submitter;
    }

    bytes private constant THING_TD = "ThingTd(bytes16 id)";
    bytes32 private immutable i_thingTdHash;
    bytes private constant SETTLEMENT_PROPOSAL_TD =
        "SettlementProposalTd(bytes16 thingId,bytes16 id)";
    bytes32 private immutable i_settlementProposalTdHash;
    bytes32 private constant SALT =
        0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558;
    bytes private constant DOMAIN_TD =
        "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract,bytes32 salt)";
    bytes32 private immutable i_domainSeparator;

    Truthserum private immutable i_truthserum;
    ThingSubmissionVerifierLottery public s_thingSubmissionVerifierLottery;
    AcceptancePoll public s_acceptancePoll;
    ThingAssessmentVerifierLottery public s_thingAssessmentVerifierLottery;
    AssessmentPoll public s_assessmentPoll;
    address private s_orchestrator;

    uint256 private s_thingSubmissionStake;
    uint256 private s_verifierStake;
    uint256 private s_thingSettlementProposalStake;
    uint256 private s_thingSubmissionAcceptedReward;
    uint256 private s_thingSubmissionRejectedPenalty;
    uint256 private s_verifierReward;
    uint256 private s_verifierPenalty;
    uint256 private s_thingSettlementProposalAcceptedReward;
    uint256 private s_thingSettlementProposalRejectedPenalty;

    mapping(address => uint256) public s_balanceOf;
    mapping(address => uint256) public s_stakedBalanceOf;

    mapping(bytes16 => address) public s_thingSubmitter;

    mapping(bytes16 => SettlementProposal) public s_thingIdToSettlementProposal;

    event FundsDeposited(address indexed user, uint256 amount);

    event ThingFunded(
        bytes16 indexed thingId,
        address indexed user,
        uint256 thingStake
    );

    event ThingSettlementProposalFunded(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        uint256 thingSettlementProposalStake
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
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyVerifierLottery() {
        // if (
        //     msg.sender != address(s_thingSubmissionVerifierLottery) &&
        //     msg.sender != address(s_thingAssessmentVerifierLottery)
        // ) {
        //     revert TruQuest__Unauthorized();
        // }
        _;
    }

    modifier onlyPoll() {
        // if (
        //     msg.sender != address(s_acceptancePoll) &&
        //     msg.sender != address(s_assessmentPoll)
        // ) {
        //     revert TruQuest__Unauthorized();
        // }
        _;
    }

    modifier onlyAcceptancePoll() {
        // if (msg.sender != address(s_acceptancePoll)) {
        //     revert TruQuest__Unauthorized();
        // }
        _;
    }

    modifier onlyAssessmentPoll() {
        // if (msg.sender != address(s_assessmentPoll)) {
        //     revert TruQuest__Unauthorized();
        // }
        _;
    }

    modifier onlyLotteryOrPoll() {
        // if (
        //     !(msg.sender == address(s_thingSubmissionVerifierLottery) ||
        //         msg.sender == address(s_thingAssessmentVerifierLottery) ||
        //         msg.sender == address(s_acceptancePoll) ||
        //         msg.sender == address(s_assessmentPoll))
        // ) {
        //     revert TruQuest__Unauthorized();
        // }
        _;
    }

    modifier onlyWhenNotFunded(bytes16 _thingId) {
        if (s_thingSubmitter[_thingId] != address(0)) {
            revert TruQuest__ThingAlreadyFunded(_thingId);
        }
        _;
    }

    modifier onlyWhenNoProposalUnderAssessmentFor(bytes16 _thingId) {
        if (s_thingIdToSettlementProposal[_thingId].submitter != address(0)) {
            revert TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
                _thingId
            );
        }
        _;
    }

    constructor(
        address _truthserumAddress,
        uint8 _numVerifiers,
        uint256 _verifierStake,
        uint256 _thingSubmissionStake,
        uint256 _thingSubmissionAcceptedReward,
        uint256 _thingSubmissionRejectedPenalty,
        uint256 _verifierReward,
        uint256 _verifierPenalty,
        uint16 _verifierLotteryDurationBlocks,
        uint16 _pollDurationBlocks,
        uint256 _thingSettlementProposalStake
    ) {
        // uint256 _thingSettlementProposalAcceptedReward,
        // uint256 _thingSettlementProposalRejectedPenalty,
        // uint8 _votingVolumeThresholdPercent,
        // uint8 _majorityThresholdPercent
        uint256 _thingSettlementProposalAcceptedReward = 7;
        uint256 _thingSettlementProposalRejectedPenalty = 3;
        uint8 _votingVolumeThresholdPercent = 50;
        uint8 _majorityThresholdPercent = 51;

        i_truthserum = Truthserum(_truthserumAddress);
        // s_thingSubmissionVerifierLottery = new ThingSubmissionVerifierLottery(
        //     address(this),
        //     _numVerifiers,
        //     _verifierLotteryDurationBlocks
        // );
        // s_acceptancePoll = new AcceptancePoll(
        //     address(this),
        //     _pollDurationBlocks,
        //     _votingVolumeThresholdPercent,
        //     _majorityThresholdPercent
        // );
        // s_thingAssessmentVerifierLottery = new ThingAssessmentVerifierLottery(
        //     address(this),
        //     _numVerifiers,
        //     _verifierLotteryDurationBlocks
        // );
        // s_assessmentPoll = new AssessmentPoll(
        //     address(this),
        //     _pollDurationBlocks,
        //     _votingVolumeThresholdPercent,
        //     _majorityThresholdPercent
        // );
        // s_thingSubmissionVerifierLottery.connectToAcceptancePoll(
        //     address(s_acceptancePoll)
        // );
        // s_acceptancePoll.connectToThingSubmissionVerifierLottery(
        //     address(s_thingSubmissionVerifierLottery)
        // );
        // s_thingAssessmentVerifierLottery.connectToAcceptancePoll(
        //     address(s_acceptancePoll)
        // );
        // s_thingAssessmentVerifierLottery.connectToAssessmentPoll(
        //     address(s_assessmentPoll)
        // );
        // s_assessmentPoll.connectToThingAssessmentVerifierLottery(
        //     address(s_thingAssessmentVerifierLottery)
        // );

        s_orchestrator = msg.sender;
        s_thingSubmissionStake = _thingSubmissionStake;
        s_verifierStake = _verifierStake;
        s_thingSubmissionAcceptedReward = _thingSubmissionAcceptedReward;
        s_thingSubmissionRejectedPenalty = _thingSubmissionRejectedPenalty;
        s_verifierReward = _verifierReward;
        s_verifierPenalty = _verifierPenalty;
        s_thingSettlementProposalStake = _thingSettlementProposalStake;
        s_thingSettlementProposalAcceptedReward = _thingSettlementProposalAcceptedReward;
        s_thingSettlementProposalRejectedPenalty = _thingSettlementProposalRejectedPenalty;

        i_domainSeparator = keccak256(
            abi.encode(
                keccak256(DOMAIN_TD),
                keccak256("TruQuest"),
                keccak256("0.0.1"),
                block.chainid,
                address(this),
                SALT
            )
        );
        i_thingTdHash = keccak256(THING_TD);
        i_settlementProposalTdHash = keccak256(SETTLEMENT_PROPOSAL_TD);
    }

    function deposit(uint256 _amount) public {
        i_truthserum.transferFrom(msg.sender, address(this), _amount);
        s_balanceOf[msg.sender] += _amount;
        emit FundsDeposited(msg.sender, _amount);
    }

    function _stake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] += _amount;
    }

    function _unstake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] -= _amount;
    }

    function stakeAsVerifier(address _user) external onlyVerifierLottery {
        s_stakedBalanceOf[_user] += s_verifierStake;
    }

    function unstakeAsVerifier(address _user) external onlyLotteryOrPoll {
        s_stakedBalanceOf[_user] -= s_verifierStake;
    }

    // @@TODO!!: Make more restrictive!
    function unstakeThingSubmitter(address _user) external onlyLotteryOrPoll {
        _unstake(_user, s_thingSubmissionStake);
    }

    // @@TODO!!: Make more restrictive!
    function unstakeProposalSubmitter(
        address _user
    ) external onlyLotteryOrPoll {
        _unstake(_user, s_thingSettlementProposalStake);
    }

    function unstakeAndRewardThingSubmitter(
        address _user
    ) external onlyAcceptancePoll {
        _unstake(_user, s_thingSubmissionStake);
        s_balanceOf[_user] += s_thingSubmissionAcceptedReward;
    }

    function unstakeAndSlashThingSubmitter(
        address _user
    ) external onlyAcceptancePoll {
        _unstake(_user, s_thingSubmissionStake);
        s_balanceOf[_user] -= s_thingSubmissionRejectedPenalty;
    }

    function unstakeAndRewardVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        s_balanceOf[_user] += s_verifierReward;
    }

    function unstakeAndSlashVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        s_balanceOf[_user] -= s_verifierPenalty;
    }

    function unstakeAndRewardProposalSubmitter(
        address _user
    ) external onlyAssessmentPoll {
        _unstake(_user, s_thingSettlementProposalStake);
        s_balanceOf[_user] += s_thingSettlementProposalAcceptedReward;
    }

    function unstakeAndSlashProposalSubmitter(
        address _user
    ) external onlyAssessmentPoll {
        _unstake(_user, s_thingSettlementProposalStake);
        s_balanceOf[_user] -= s_thingSettlementProposalRejectedPenalty;
    }

    function getAvailableFunds(address _user) public view returns (uint256) {
        return s_balanceOf[_user] - s_stakedBalanceOf[_user];
    }

    function checkHasEnoughFundsToStakeAsVerifier(
        address _user
    ) external view returns (bool) {
        return getAvailableFunds(_user) >= s_verifierStake;
    }

    function _hashThing(
        ThingTd calldata _thing
    ) private view returns (bytes32) {
        return
            keccak256(
                abi.encodePacked(
                    "\x19\x01",
                    i_domainSeparator,
                    keccak256(abi.encode(i_thingTdHash, _thing.id))
                )
            );
    }

    function _verifyOrchestratorSignatureForThing(
        ThingTd calldata _thing,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    ) private view returns (bool) {
        return s_orchestrator == ecrecover(_hashThing(_thing), _v, _r, _s);
    }

    function checkThingAlreadyFunded(
        bytes16 _thingId
    ) public view returns (bool) {
        return s_thingSubmitter[_thingId] != address(0);
    }

    function fundThing(
        ThingTd calldata _thing,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        public
        onlyWhenNotFunded(_thing.id)
        whenHasAtLeast(s_thingSubmissionStake)
    {
        if (!_verifyOrchestratorSignatureForThing(_thing, _v, _r, _s)) {
            revert TruQuest__InvalidSignature();
        }
        _stake(msg.sender, s_thingSubmissionStake);
        s_thingSubmitter[_thing.id] = msg.sender;
        emit ThingFunded(_thing.id, msg.sender, s_thingSubmissionStake);
    }

    function _hashSettlementProposal(
        SettlementProposalTd calldata _settlementProposal
    ) private view returns (bytes32) {
        return
            keccak256(
                abi.encodePacked(
                    "\x19\x01",
                    i_domainSeparator,
                    keccak256(
                        abi.encode(
                            i_settlementProposalTdHash,
                            _settlementProposal.thingId,
                            _settlementProposal.id
                        )
                    )
                )
            );
    }

    function _verifyOrchestratorSignatureForSettlementProposal(
        SettlementProposalTd calldata _settlementProposal,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    ) private view returns (bool) {
        return
            s_orchestrator ==
            ecrecover(_hashSettlementProposal(_settlementProposal), _v, _r, _s);
    }

    function checkThingAlreadyHasSettlementProposalUnderAssessment(
        bytes16 _thingId
    ) public view returns (bool) {
        return s_thingIdToSettlementProposal[_thingId].submitter != address(0);
    }

    function fundThingSettlementProposal(
        SettlementProposalTd calldata _settlementProposal,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        public
        onlyWhenNoProposalUnderAssessmentFor(_settlementProposal.thingId)
        whenHasAtLeast(s_thingSettlementProposalStake)
    {
        if (
            !_verifyOrchestratorSignatureForSettlementProposal(
                _settlementProposal,
                _v,
                _r,
                _s
            )
        ) {
            revert TruQuest__InvalidSignature();
        }

        _stake(msg.sender, s_thingSettlementProposalStake);
        s_thingIdToSettlementProposal[
            _settlementProposal.thingId
        ] = SettlementProposal(_settlementProposal.id, msg.sender);

        emit ThingSettlementProposalFunded(
            _settlementProposal.thingId,
            _settlementProposal.id,
            msg.sender,
            s_thingSettlementProposalStake
        );
    }

    // only...?
    function getSettlementProposalId(
        bytes16 _thingId
    ) public view returns (bytes16) {
        return s_thingIdToSettlementProposal[_thingId].id;
    }

    function getSettlementProposalSubmitter(
        bytes16 _thingId
    ) public view returns (address) {
        return s_thingIdToSettlementProposal[_thingId].submitter;
    }
}
