// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";

error VerifierLottery__NotOrchestrator();
error VerifierLottery__NotTruQuest();
error VerifierLottery__AlreadyCommittedToLottery(string thingId);
error VerifierLottery__LotteryNotActive(string thingId);
error VerifierLottery__NotEnoughFunds(uint256 requiredFunds);
error VerifierLottery__NotCommittedToLottery(string thingId);
error VerifierLottery__AlreadyJoinedLottery(string thingId);
error VerifierLottery__InvalidVerifierLotteryReveal(string thingId);
error VerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(string thingId);
error VerifierLottery__InvalidNumberOfLotteryWinners(
    uint8 numRequiredVerifiers,
    uint256 numWinners
);
error VerifierLottery__InitAndCloseLotteryInTheSameBlock(string thingId);

error VerifierLottery__SubLotteryNotActive(string thingId);
error VerifierLottery__AlreadyCommittedToSubLottery(string thingId);
error VerifierLottery__NotCommittedToSubLottery(string thingId);
error VerifierLottery__AlreadyJoinedSubLottery(string thingId);
error VerifierLottery__InvalidVerifierSubLotteryReveal(string thingId);
error VerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(string thingId);
error VerifierLottery__InitAndCloseSubLotteryInTheSameBlock(string thingId);

contract VerifierLottery {
    struct LotteryCommitment {
        bytes32 dataHash;
        int64 block;
        bool revealed;
    }

    TruQuest private immutable i_truQuest;
    address private s_orchestrator;

    uint8 private s_numVerifiers;
    uint256 private s_verifierStake;
    uint16 private s_verifierLotteryDurationBlocks;

    mapping(string => mapping(address => LotteryCommitment))
        private s_thingIdToLotteryCommitments;
    mapping(string => address[]) private s_lotteryParticipants;
    mapping(string => mapping(address => LotteryCommitment))
        private s_thingIdToSubLotteryCommitments;

    event VerifierLotteryInitiated(
        string indexed thingId,
        address orchestrator,
        bytes32 dataHash
    );
    event PreJoinedVerifierLottery(
        string indexed thingId,
        address indexed user,
        bytes32 dataHash
    );
    event JoinedVerifierLottery(
        string indexed thingId,
        address indexed user,
        uint256 nonce
    );
    event VerifierLotteryClosedWithSuccess(
        string indexed thingId,
        address orchestrator,
        uint256 nonce,
        address[] winners,
        uint256 block
    );
    event VerifierLotteryClosedInFailure(
        string indexed thingId,
        address orchestrator
    );
    event VerifierSubLotteryInitiated(
        string indexed thingId,
        address orchestrator,
        bytes32 dataHash
    );
    event PreJoinedVerifierSubLottery(
        string indexed thingId,
        address indexed user,
        bytes32 dataHash
    );
    event JoinedVerifierSubLottery(
        string indexed thingId,
        address indexed user,
        uint256 nonce
    );
    event VerifierSubLotteryClosedWithSuccess(
        string indexed thingId,
        address orchestrator,
        uint256 nonce,
        address[] winners,
        uint256 block
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert VerifierLottery__NotOrchestrator();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert VerifierLottery__NotTruQuest();
        }
        _;
    }

    modifier whenHasAtLeast(uint256 _requiredFunds) {
        if (!i_truQuest.checkHasAtLeast(msg.sender, _requiredFunds)) {
            revert VerifierLottery__NotEnoughFunds(_requiredFunds);
        }
        _;
    }

    modifier onlyOncePerLottery(string calldata _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert VerifierLottery__AlreadyCommittedToLottery(_thingId);
        }
        _;
    }

    modifier onlyWhenLotteryActive(string calldata _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block < 1) {
            revert VerifierLottery__LotteryNotActive(_thingId);
        }
        _;
    }

    modifier onlyWhenSubLotteryActive(string calldata _thingId) {
        if (
            s_thingIdToSubLotteryCommitments[_thingId][address(i_truQuest)]
                .block < 1
        ) {
            revert VerifierLottery__SubLotteryNotActive(_thingId);
        }
        _;
    }

    modifier onlyOncePerSubLottery(string calldata _thingId) {
        if (s_thingIdToSubLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert VerifierLottery__AlreadyCommittedToSubLottery(_thingId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint8 _numVerifiers,
        uint256 _verifierStake,
        uint16 _verifierLotteryDurationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_numVerifiers = _numVerifiers;
        s_verifierStake = _verifierStake;
        s_verifierLotteryDurationBlocks = _verifierLotteryDurationBlocks;
    }

    function computeHash(bytes32 _data) public view returns (bytes32) {
        return keccak256(abi.encodePacked(address(this), _data));
    }

    // onlyFunded
    function initVerifierLottery(string calldata _thingId, bytes32 _dataHash)
        public
        onlyOrchestrator
        onlyOncePerLottery(_thingId)
    {
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = LotteryCommitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );
        emit VerifierLotteryInitiated(_thingId, s_orchestrator, _dataHash);
    }

    function preJoinVerifierLottery(string calldata _thingId, bytes32 _dataHash)
        public
        onlyWhenLotteryActive(_thingId)
        whenHasAtLeast(s_verifierStake)
        onlyOncePerLottery(_thingId)
    {
        // check that current block is not too late?
        i_truQuest.stake(msg.sender, s_verifierStake);
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = LotteryCommitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );
        s_lotteryParticipants[_thingId].push(msg.sender);
        emit PreJoinedVerifierLottery(_thingId, msg.sender, _dataHash);
    }

    function joinVerifierLottery(string calldata _thingId, bytes32 _data)
        public
        onlyWhenLotteryActive(_thingId)
    {
        // check that current block is not too late?
        LotteryCommitment memory commitment = s_thingIdToLotteryCommitments[
            _thingId
        ][msg.sender];

        if (commitment.block == 0) {
            revert VerifierLottery__NotCommittedToLottery(_thingId);
        }
        if (commitment.revealed) {
            revert VerifierLottery__AlreadyJoinedLottery(_thingId);
        }
        s_thingIdToLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert VerifierLottery__InvalidVerifierLotteryReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert VerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
                _thingId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data)));

        emit JoinedVerifierLottery(_thingId, msg.sender, nonce);
    }

    function closeVerifierLotteryWithSuccess(
        string calldata _thingId,
        bytes32 _data,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingId) {
        if (_winnerIndices.length != s_numVerifiers) {
            revert VerifierLottery__InvalidNumberOfLotteryWinners(
                s_numVerifiers,
                _winnerIndices.length
            );
        }

        LotteryCommitment memory commitment = s_thingIdToLotteryCommitments[
            _thingId
        ][msg.sender];

        if (computeHash(_data) != commitment.dataHash) {
            revert VerifierLottery__InvalidVerifierLotteryReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert VerifierLottery__InitAndCloseLotteryInTheSameBlock(_thingId);
        }

        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        uint64 j = 0;
        address[] memory lotteryParticipants = s_lotteryParticipants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint8 i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = lotteryParticipants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(lotteryParticipants[j], s_verifierStake);
            }
            ++j;
        }

        s_lotteryParticipants[_thingId] = new address[](0); // unnecessary?
        delete s_lotteryParticipants[_thingId];

        i_truQuest.initAcceptancePoll(_thingId, winners);

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data)));

        emit VerifierLotteryClosedWithSuccess(
            _thingId,
            s_orchestrator,
            nonce,
            winners,
            block.number
        );
    }

    // when not enough participants
    // @@??: add reason string/enum param ?
    function closeVerifierLotteryInFailure(string calldata _thingId)
        public
        onlyOrchestrator
        onlyWhenLotteryActive(_thingId)
    {
        // checks?
        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        address[] memory lotteryParticipants = s_lotteryParticipants[_thingId];
        for (uint64 i = 0; i < lotteryParticipants.length; ++i) {
            i_truQuest.unstake(lotteryParticipants[i], s_verifierStake);
        }

        s_lotteryParticipants[_thingId] = new address[](0); // unnecessary?
        delete s_lotteryParticipants[_thingId];

        emit VerifierLotteryClosedInFailure(_thingId, s_orchestrator);
    }

    function initVerifierSubLottery(string calldata _thingId, bytes32 _dataHash)
        external
        onlyTruQuest
        onlyOncePerSubLottery(_thingId)
    {
        s_thingIdToSubLotteryCommitments[_thingId][
            msg.sender
        ] = LotteryCommitment(_dataHash, int64(uint64(block.number)), false);

        emit VerifierSubLotteryInitiated(_thingId, s_orchestrator, _dataHash);
    }

    function preJoinVerifierSubLottery(
        string calldata _thingId,
        bytes32 _dataHash
    )
        public
        onlyWhenSubLotteryActive(_thingId)
        whenHasAtLeast(s_verifierStake)
        onlyOncePerSubLottery(_thingId)
    {
        // check that current block is not too late?
        i_truQuest.stake(msg.sender, s_verifierStake);
        s_thingIdToSubLotteryCommitments[_thingId][
            msg.sender
        ] = LotteryCommitment(_dataHash, int64(uint64(block.number)), false);
        s_lotteryParticipants[_thingId].push(msg.sender);

        emit PreJoinedVerifierSubLottery(_thingId, msg.sender, _dataHash);
    }

    function joinVerifierSubLottery(string calldata _thingId, bytes32 _data)
        public
        onlyWhenSubLotteryActive(_thingId)
    {
        // check that current block is not too late?
        LotteryCommitment memory commitment = s_thingIdToSubLotteryCommitments[
            _thingId
        ][msg.sender];

        if (commitment.block == 0) {
            revert VerifierLottery__NotCommittedToSubLottery(_thingId);
        }
        if (commitment.revealed) {
            revert VerifierLottery__AlreadyJoinedSubLottery(_thingId);
        }
        s_thingIdToSubLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert VerifierLottery__InvalidVerifierSubLotteryReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert VerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
                _thingId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data)));

        emit JoinedVerifierSubLottery(_thingId, msg.sender, nonce);
    }

    function closeVerifierSubLotteryWithSuccess(
        string calldata _thingId,
        bytes32 _data,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenSubLotteryActive(_thingId) {
        // uint8 numSubstituteVerifiers = uint8(
        //     s_numVerifiers - i_truQuest.s_thingVerifiers[_thingId].length
        // );
        // if (_winnerIndices.length != numSubstituteVerifiers) {
        //     revert VerifierLottery__InvalidNumberOfLotteryWinners(
        //         numSubstituteVerifiers,
        //         _winnerIndices.length
        //     );
        // }

        LotteryCommitment memory commitment = s_thingIdToSubLotteryCommitments[
            _thingId
        ][address(i_truQuest)];
        if (computeHash(_data) != commitment.dataHash) {
            revert VerifierLottery__InvalidVerifierSubLotteryReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert VerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
                _thingId
            );
        }

        s_thingIdToSubLotteryCommitments[_thingId][address(i_truQuest)]
            .block = -1;

        uint64 j = 0;
        address[] memory lotteryParticipants = s_lotteryParticipants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint8 i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = lotteryParticipants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(lotteryParticipants[j], s_verifierStake);
            }
            ++j;
        }

        s_lotteryParticipants[_thingId] = new address[](0); // unnecessary?
        delete s_lotteryParticipants[_thingId];

        i_truQuest.initAcceptanceSubPoll(_thingId, winners);

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data)));

        emit VerifierSubLotteryClosedWithSuccess(
            _thingId,
            s_orchestrator,
            nonce,
            winners,
            block.number
        );
    }
}
