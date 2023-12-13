// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "./Truthserum.sol";
import "./RestrictedAccess.sol";

error TruQuest__TheWorldIsStopped();
error TruQuest__WithdrawalsDisabled();
error TruQuest__ThingAlreadyFunded(bytes16 thingId);
error TruQuest__NotEnoughFunds(uint256 requiredAmount, uint256 availableAmount);
error TruQuest__Unauthorized();
error TruQuest__InvalidSignature();
error TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
    bytes16 thingId
);
error TruQuest__RequestedWithdrawAmountExceedsAvailable(
    uint256 requestedAmount,
    uint256 availableAmount
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

    bytes private constant VERSION = "0.1.0";

    Truthserum private immutable i_truthserum;
    address private s_thingValidationVerifierLotteryAddress;
    address private s_thingValidationPollAddress;
    address private s_settlementProposalAssessmentVerifierLotteryAddress;
    address private s_settlementProposalAssessmentPollAddress;
    RestrictedAccess private s_restrictedAccess;

    address private s_orchestrator;

    uint256 public s_thingStake;
    uint256 public s_verifierStake;
    uint256 public s_settlementProposalStake;
    uint256 public s_thingAcceptedReward;
    uint256 public s_thingRejectedPenalty;
    uint256 public s_verifierReward;
    uint256 public s_verifierPenalty;
    uint256 public s_settlementProposalAcceptedReward;
    uint256 public s_settlementProposalRejectedPenalty;

    // @@NOTE: When 'true', no longer accept deposit, withdraw, and fund requests, so that once
    // all currently active evaluation processes (lotteries and polls) finish, we can safely
    // export all data from the contracts and import it into new ones. Used to migrate data
    // between contract versions on the testnet. Will be removed (along with import/export functions
    // and some fields that are only maintained for migration purposes) from the mainnet version.
    bool public s_stopTheWorld;
    bool public s_withdrawalsEnabled = false;

    // @@??: Use it to deposit ephemeral funds for new users? Cannot withdraw, only use on the platform?
    uint256 private s_treasury;

    address[] private s_users;
    mapping(address => bool) private s_onboardedUsers;
    mapping(address => uint256) public s_balanceOf;
    mapping(address => uint256) public s_stakedBalanceOf;

    bytes16[] private s_fundedThings;
    mapping(bytes16 => address) public s_thingSubmitter;
    bytes16[] private s_thingsWithFundedSettlementProposal;
    mapping(bytes16 => SettlementProposal) public s_thingIdToSettlementProposal;

    event FundsDeposited(address indexed user, uint256 amount);

    event FundsWithdrawn(address indexed user, uint256 amount);

    event ThingFunded(
        bytes16 indexed thingId,
        address indexed user,
        uint256 thingStake
    );

    event SettlementProposalFunded(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        uint256 settlementProposalStake
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
        if (
            msg.sender != s_thingValidationVerifierLotteryAddress &&
            msg.sender != s_settlementProposalAssessmentVerifierLotteryAddress
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyPoll() {
        if (
            msg.sender != s_thingValidationPollAddress &&
            msg.sender != s_settlementProposalAssessmentPollAddress
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyThingValidationPoll() {
        if (msg.sender != s_thingValidationPollAddress) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlySettlementProposalAssessmentPoll() {
        if (msg.sender != s_settlementProposalAssessmentPollAddress) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyLotteryOrPoll() {
        if (
            !(msg.sender == s_thingValidationVerifierLotteryAddress ||
                msg.sender ==
                s_settlementProposalAssessmentVerifierLotteryAddress ||
                msg.sender == s_thingValidationPollAddress ||
                msg.sender == s_settlementProposalAssessmentPollAddress)
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyWhenNotFunded(bytes16 _thingId) {
        if (s_thingSubmitter[_thingId] != address(0)) {
            revert TruQuest__ThingAlreadyFunded(_thingId);
        }
        _;
    }

    modifier onlyWhenNoSettlementProposalUnderAssessmentFor(bytes16 _thingId) {
        if (s_thingIdToSettlementProposal[_thingId].submitter != address(0)) {
            revert TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
                _thingId
            );
        }
        _;
    }

    modifier onlyWhenTheWorldIsSpinning() {
        if (s_stopTheWorld) {
            revert TruQuest__TheWorldIsStopped();
        }
        _;
    }

    modifier onlyIfWhitelisted() {
        if (!s_restrictedAccess.checkHasAccess(msg.sender)) {
            revert RestrictedAccess__Forbidden();
        }
        _;
    }

    constructor(
        address _truthserumAddress,
        uint256 _verifierStake,
        uint256 _verifierReward,
        uint256 _verifierPenalty,
        uint256 _thingStake,
        uint256 _thingAcceptedReward,
        uint256 _thingRejectedPenalty,
        uint256 _settlementProposalStake,
        uint256 _settlementProposalAcceptedReward,
        uint256 _settlementProposalRejectedPenalty
    ) {
        i_truthserum = Truthserum(_truthserumAddress);
        s_orchestrator = msg.sender;
        s_verifierStake = _verifierStake;
        s_verifierReward = _verifierReward;
        s_verifierPenalty = _verifierPenalty;
        s_thingStake = _thingStake;
        s_thingAcceptedReward = _thingAcceptedReward;
        s_thingRejectedPenalty = _thingRejectedPenalty;
        s_settlementProposalStake = _settlementProposalStake;
        s_settlementProposalAcceptedReward = _settlementProposalAcceptedReward;
        s_settlementProposalRejectedPenalty = _settlementProposalRejectedPenalty;

        i_domainSeparator = keccak256(
            abi.encode(
                keccak256(DOMAIN_TD),
                keccak256("TruQuest"),
                keccak256(VERSION),
                block.chainid,
                address(this),
                SALT
            )
        );
        i_thingTdHash = keccak256(THING_TD);
        i_settlementProposalTdHash = keccak256(SETTLEMENT_PROPOSAL_TD);
    }

    function setThingStake(uint256 _thingStake) external onlyOrchestrator {
        s_thingStake = _thingStake;
    }

    function setVerifierStake(
        uint256 _verifierStake
    ) external onlyOrchestrator {
        s_verifierStake = _verifierStake;
    }

    function setSettlementProposalStake(
        uint256 _settlementProposalStake
    ) external onlyOrchestrator {
        s_settlementProposalStake = _settlementProposalStake;
    }

    function setThingAcceptedReward(
        uint256 _thingAcceptedReward
    ) external onlyOrchestrator {
        s_thingAcceptedReward = _thingAcceptedReward;
    }

    function setThingRejectedPenalty(
        uint256 _thingRejectedPenalty
    ) external onlyOrchestrator {
        s_thingRejectedPenalty = _thingRejectedPenalty;
    }

    function setVerifierReward(
        uint256 _verifierReward
    ) external onlyOrchestrator {
        s_verifierReward = _verifierReward;
    }

    function setVerifierPenalty(
        uint256 _verifierPenalty
    ) external onlyOrchestrator {
        s_verifierPenalty = _verifierPenalty;
    }

    function setSettlementProposalAcceptedReward(
        uint256 _settlementProposalAcceptedReward
    ) external onlyOrchestrator {
        s_settlementProposalAcceptedReward = _settlementProposalAcceptedReward;
    }

    function setSettlementProposalRejectedPenalty(
        uint256 _settlementProposalRejectedPenalty
    ) external onlyOrchestrator {
        s_settlementProposalRejectedPenalty = _settlementProposalRejectedPenalty;
    }

    function getVersion() external pure returns (string memory) {
        return string(VERSION);
    }

    function setLotteryAndPollAddresses(
        address _thingValidationVerifierLotteryAddress,
        address _thingValidationPollAddress,
        address _settlementProposalAssessmentVerifierLotteryAddress,
        address _settlementProposalAssessmentPollAddress
    ) external onlyOrchestrator {
        s_thingValidationVerifierLotteryAddress = _thingValidationVerifierLotteryAddress;
        s_thingValidationPollAddress = _thingValidationPollAddress;
        s_settlementProposalAssessmentVerifierLotteryAddress = _settlementProposalAssessmentVerifierLotteryAddress;
        s_settlementProposalAssessmentPollAddress = _settlementProposalAssessmentPollAddress;
    }

    function setRestrictedAccess(
        address _restrictedAccessAddress
    ) external onlyOrchestrator {
        s_restrictedAccess = RestrictedAccess(_restrictedAccessAddress);
    }

    function checkHasAccess(address _user) external view returns (bool) {
        return s_restrictedAccess.checkHasAccess(_user);
    }

    function stopTheWorld(bool _value) external onlyOrchestrator {
        s_stopTheWorld = _value;
    }

    function enableWithdrawals(bool _value) external onlyOrchestrator {
        s_withdrawalsEnabled = _value;
    }

    function exportUsersAndBalances()
        external
        view
        returns (
            address[] memory users,
            uint256[] memory balances,
            uint256[] memory stakedBalances
        )
    {
        users = s_users;
        balances = new uint256[](users.length);
        stakedBalances = new uint256[](users.length);
        for (uint256 i = 0; i < users.length; ++i) {
            balances[i] = s_balanceOf[users[i]];
            stakedBalances[i] = s_stakedBalanceOf[users[i]];
        }
    }

    function importUsersAndBalances(
        address[] calldata _users,
        uint256[] calldata _balances,
        uint256[] calldata _stakedBalances
    ) external onlyOrchestrator {
        s_users = _users;
        for (uint256 i = 0; i < _users.length; ++i) {
            address user = _users[i];
            s_onboardedUsers[user] = true;
            s_balanceOf[user] = _balances[i];
            s_stakedBalanceOf[user] = _stakedBalances[i];
        }
    }

    function exportThingSubmitter()
        external
        view
        returns (bytes16[] memory thingIds, address[] memory submitters)
    {
        thingIds = s_fundedThings;
        submitters = new address[](thingIds.length);
        for (uint256 i = 0; i < submitters.length; ++i) {
            submitters[i] = s_thingSubmitter[thingIds[i]];
        }
    }

    function importThingSubmitter(
        bytes16[] calldata _thingIds,
        address[] calldata _submitters
    ) external onlyOrchestrator {
        s_fundedThings = _thingIds;
        for (uint256 i = 0; i < _thingIds.length; ++i) {
            s_thingSubmitter[_thingIds[i]] = _submitters[i];
        }
    }

    function exportThingIdToSettlementProposal()
        external
        view
        returns (
            bytes16[] memory thingIds,
            SettlementProposal[] memory settlementProposals
        )
    {
        // @@!!: Keep in mind that we don't remove the thing from s_thingsWithFundedSettlementProposal array
        // when a proposal gets archived/declined/etc, but we /do/ remove the corresponding entry from
        // s_thingIdToSettlementProposal mapping to allow for future proposals. This means that there could
        // be duplicates in s_thingsWithFundedSettlementProposal array. This is fine so long as we take care
        // of them on the client side.
        thingIds = s_thingsWithFundedSettlementProposal;
        settlementProposals = new SettlementProposal[](thingIds.length);
        for (uint256 i = 0; i < settlementProposals.length; ++i) {
            settlementProposals[i] = s_thingIdToSettlementProposal[thingIds[i]];
        }
    }

    function importThingIdToSettlementProposal(
        bytes16[] calldata _thingIds,
        SettlementProposal[] calldata _settlementProposals
    ) external onlyOrchestrator {
        s_thingsWithFundedSettlementProposal = _thingIds;
        for (uint256 i = 0; i < _thingIds.length; ++i) {
            s_thingIdToSettlementProposal[_thingIds[i]] = _settlementProposals[
                i
            ];
        }
    }

    function mintAndDepositTruthserumTo(
        address _user,
        uint256 _amount
    ) external onlyOrchestrator {
        i_truthserum.mint(_amount);
        if (!s_onboardedUsers[_user]) {
            s_onboardedUsers[_user] = true;
            s_users.push(_user);
        }
        s_balanceOf[_user] += _amount;
        emit FundsDeposited(_user, _amount);
    }

    function deposit(
        uint256 _amount
    ) external onlyWhenTheWorldIsSpinning onlyIfWhitelisted {
        i_truthserum.transferFrom(msg.sender, address(this), _amount);
        if (!s_onboardedUsers[msg.sender]) {
            s_onboardedUsers[msg.sender] = true;
            s_users.push(msg.sender);
        }
        s_balanceOf[msg.sender] += _amount;
        emit FundsDeposited(msg.sender, _amount);
    }

    function withdraw(
        uint256 _amount
    ) external onlyWhenTheWorldIsSpinning onlyIfWhitelisted {
        if (!s_withdrawalsEnabled) {
            revert TruQuest__WithdrawalsDisabled();
        }
        
        uint256 availableAmount = getAvailableFunds(msg.sender);
        if (availableAmount < _amount) {
            revert TruQuest__RequestedWithdrawAmountExceedsAvailable(
                _amount,
                availableAmount
            );
        }

        i_truthserum.transfer(msg.sender, _amount);
        s_balanceOf[msg.sender] -= _amount;

        emit FundsWithdrawn(msg.sender, _amount);
    }

    function _stake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] += _amount;
    }

    function _unstake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] -= _amount;
    }

    function _reward(address _user, uint256 _amount) private {
        i_truthserum.mint(_amount);
        s_balanceOf[_user] += _amount;
    }

    function _slash(address _user, uint256 _amount) private {
        s_treasury += _amount;
        s_balanceOf[_user] -= _amount;
    }

    function stakeAsVerifier(address _user) external onlyVerifierLottery {
        _stake(_user, s_verifierStake);
    }

    function unstakeAsVerifier(address _user) external onlyLotteryOrPoll {
        _unstake(_user, s_verifierStake);
    }

    function unstakeThingSubmitter(address _user) external onlyLotteryOrPoll {
        _unstake(_user, s_thingStake);
    }

    function unstakeSettlementProposalSubmitter(
        address _user
    ) external onlyLotteryOrPoll {
        _unstake(_user, s_settlementProposalStake);
    }

    function unstakeAndRewardThingSubmitter(
        address _user
    ) external onlyThingValidationPoll {
        _unstake(_user, s_thingStake);
        _reward(_user, s_thingAcceptedReward);
    }

    function unstakeAndSlashThingSubmitter(
        address _user
    ) external onlyThingValidationPoll {
        _unstake(_user, s_thingStake);
        _slash(_user, s_thingRejectedPenalty);
    }

    function unstakeAndRewardVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        _reward(_user, s_verifierReward);
    }

    function unstakeAndSlashVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        _slash(_user, s_verifierPenalty);
    }

    function unstakeAndRewardSettlementProposalSubmitter(
        address _user
    ) external onlySettlementProposalAssessmentPoll {
        _unstake(_user, s_settlementProposalStake);
        _reward(_user, s_settlementProposalAcceptedReward);
    }

    function unstakeAndSlashSettlementProposalSubmitter(
        address _user
    ) external onlySettlementProposalAssessmentPoll {
        _unstake(_user, s_settlementProposalStake);
        _slash(_user, s_settlementProposalRejectedPenalty);
    }

    function getAvailableFunds(address _user) public view returns (uint256) {
        return s_balanceOf[_user] - s_stakedBalanceOf[_user];
    }

    function checkHasEnoughFundsToStakeAsVerifier(
        address _user
    ) external view returns (bool) {
        return getAvailableFunds(_user) >= s_verifierStake;
    }

    function _hashThing(ThingTd memory _thing) private view returns (bytes32) {
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
        ThingTd memory _thing,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    ) private view returns (bool) {
        return s_orchestrator == ecrecover(_hashThing(_thing), _v, _r, _s);
    }

    function checkThingAlreadyFunded(
        bytes16 _thingId
    ) external view returns (bool) {
        return s_thingSubmitter[_thingId] != address(0);
    }

    function fundThing(
        bytes16 _thingId,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        external
        onlyWhenTheWorldIsSpinning
        onlyIfWhitelisted
        onlyWhenNotFunded(_thingId)
        whenHasAtLeast(s_thingStake)
    {
        ThingTd memory thing = ThingTd(_thingId);
        if (!_verifyOrchestratorSignatureForThing(thing, _v, _r, _s)) {
            revert TruQuest__InvalidSignature();
        }
        _stake(msg.sender, s_thingStake);
        s_fundedThings.push(_thingId);
        s_thingSubmitter[_thingId] = msg.sender;
        emit ThingFunded(_thingId, msg.sender, s_thingStake);
    }

    function _hashSettlementProposal(
        SettlementProposalTd memory _settlementProposal
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
        SettlementProposalTd memory _settlementProposal,
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
    ) external view returns (bool) {
        return s_thingIdToSettlementProposal[_thingId].submitter != address(0);
    }

    function fundSettlementProposal(
        bytes16 _thingId,
        bytes16 _settlementProposalId,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        external
        onlyWhenTheWorldIsSpinning
        onlyIfWhitelisted
        onlyWhenNoSettlementProposalUnderAssessmentFor(_thingId)
        whenHasAtLeast(s_settlementProposalStake)
    {
        SettlementProposalTd memory proposal = SettlementProposalTd(
            _thingId,
            _settlementProposalId
        );
        if (
            !_verifyOrchestratorSignatureForSettlementProposal(
                proposal,
                _v,
                _r,
                _s
            )
        ) {
            revert TruQuest__InvalidSignature();
        }

        _stake(msg.sender, s_settlementProposalStake);
        s_thingsWithFundedSettlementProposal.push(_thingId);
        s_thingIdToSettlementProposal[_thingId] = SettlementProposal(
            _settlementProposalId,
            msg.sender
        );

        emit SettlementProposalFunded(
            _thingId,
            _settlementProposalId,
            msg.sender,
            s_settlementProposalStake
        );
    }

    function setThingNoLongerHasSettlementProposalUnderAssessment(
        bytes16 _thingId
    ) external onlyLotteryOrPoll {
        delete s_thingIdToSettlementProposal[_thingId];
    }

    function getSettlementProposalId(
        bytes16 _thingId
    ) external view returns (bytes16) {
        return s_thingIdToSettlementProposal[_thingId].id;
    }

    function getSettlementProposalSubmitter(
        bytes16 _thingId
    ) external view returns (address) {
        return s_thingIdToSettlementProposal[_thingId].submitter;
    }
}
