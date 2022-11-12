// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./Truthserum.sol";
import "./VerifierLottery.sol";
import "./AcceptancePoll.sol";

error TruQuest__ThingAlreadyFunded(string thingId);
error TruQuest__NotEnoughFunds(uint256 requiredAmount, uint256 availableAmount);
error TruQuest__NotOrchestrator();
error TruQuest__NotVerifierLottery();
error TruQuest__NotAcceptancePoll();

contract TruQuest {
    Truthserum private immutable i_truthserum;
    VerifierLottery public s_verifierLottery;
    AcceptancePoll public s_acceptancePoll;
    address private s_orchestrator;

    uint256 private s_thingStake;

    mapping(address => uint256) private s_balanceOf;
    mapping(address => uint256) private s_stakedBalanceOf;

    mapping(string => address) public s_thingSubmitter;

    event FundsDeposited(address indexed user, uint256 amount);
    event ThingFunded(
        string indexed thingId,
        address indexed user,
        uint256 thingStake
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
        if (msg.sender != address(s_verifierLottery)) {
            revert TruQuest__NotVerifierLottery();
        }
        _;
    }

    modifier onlyAcceptancePoll() {
        if (msg.sender != address(s_acceptancePoll)) {
            revert TruQuest__NotAcceptancePoll();
        }
        _;
    }

    modifier onlyWhenNotFunded(string calldata _thingId) {
        if (s_thingSubmitter[_thingId] != address(0)) {
            revert TruQuest__ThingAlreadyFunded(_thingId);
        }
        _;
    }

    constructor(
        address _truthserumAddress,
        uint8 _numVerifiers,
        uint256 _verifierStake,
        uint256 _thingStake,
        uint256 _thingSubmissionAcceptedReward,
        uint256 _verifierReward,
        uint16 _verifierLotteryDurationBlocks,
        uint16 _acceptancePollDurationBlocks
    ) {
        i_truthserum = Truthserum(_truthserumAddress);
        s_verifierLottery = new VerifierLottery(
            address(this),
            _numVerifiers,
            _verifierStake,
            _verifierLotteryDurationBlocks
        );
        s_acceptancePoll = new AcceptancePoll(
            address(this),
            _thingSubmissionAcceptedReward,
            _verifierReward,
            _acceptancePollDurationBlocks
        );
        s_verifierLottery.connectToAcceptancePoll(address(s_acceptancePoll));
        s_acceptancePoll.connectToVerifierLottery(address(s_verifierLottery));
        s_orchestrator = msg.sender;
        s_thingStake = _thingStake;
    }

    function deposit(uint256 _amount) public {
        i_truthserum.transferFrom(msg.sender, address(this), _amount);
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

    function slash(address _user, uint256 _amount) external onlyAcceptancePoll {
        s_balanceOf[_user] -= _amount;
        s_stakedBalanceOf[_user] -= _amount;
    }

    function reward(address _user, uint256 _amount)
        external
        onlyAcceptancePoll
    {
        // i_truthserum.transfer(_user, _amount);
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
        _stake(msg.sender, s_thingStake);
        s_thingSubmitter[_thingId] = msg.sender;
        emit ThingFunded(_thingId, msg.sender, s_thingStake);
    }
}
