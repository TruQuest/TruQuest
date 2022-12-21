// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./AcceptancePoll.sol";

error ThingSubmissionVerifierLottery__NotOrchestrator();
error ThingSubmissionVerifierLottery__NotTruQuest();
error ThingSubmissionVerifierLottery__NotAcceptancePoll();
error ThingSubmissionVerifierLottery__AlreadyCommittedToLottery(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__LotteryNotActive(bytes16 thingId);
error ThingSubmissionVerifierLottery__LotteryExpired(bytes16 thingId);
error ThingSubmissionVerifierLottery__NotEnoughFunds(uint256 requiredFunds);
error ThingSubmissionVerifierLottery__NotCommittedToLottery(bytes16 thingId);
error ThingSubmissionVerifierLottery__AlreadyJoinedLottery(bytes16 thingId);
error ThingSubmissionVerifierLottery__InvalidLotteryReveal(bytes16 thingId);
error ThingSubmissionVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__InvalidNumberOfLotteryWinners(
    uint8 numRequiredVerifiers,
    uint256 numWinners
);
error ThingSubmissionVerifierLottery__InitAndCloseLotteryInTheSameBlock(
    bytes16 thingId
);

error ThingSubmissionVerifierLottery__SubLotteryNotActive(bytes16 thingId);
error ThingSubmissionVerifierLottery__SubLotteryExpired(bytes16 thingId);
error ThingSubmissionVerifierLottery__AlreadyCommittedToSubLottery(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__NotCommittedToSubLottery(bytes16 thingId);
error ThingSubmissionVerifierLottery__AlreadyJoinedSubLottery(bytes16 thingId);
error ThingSubmissionVerifierLottery__InvalidSubLotteryReveal(bytes16 thingId);
error ThingSubmissionVerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
    bytes16 thingId
);

contract ThingSubmissionVerifierLottery {
    struct Commitment {
        bytes32 dataHash;
        int64 block;
        bool revealed;
    }

    uint256 public constant MAX_NONCE = 1000000;

    TruQuest private immutable i_truQuest;
    AcceptancePoll private s_acceptancePoll;
    address private s_orchestrator;

    uint8 private s_numVerifiers;
    uint256 private s_verifierStake;
    uint16 private s_durationBlocks;

    mapping(bytes16 => mapping(address => Commitment))
        private s_thingIdToLotteryCommitments;
    mapping(bytes16 => address[]) private s_participants;
    mapping(bytes16 => mapping(address => Commitment))
        private s_thingIdToSubLotteryCommitments;

    event LotteryInitiated(
        bytes16 indexed thingId,
        address orchestrator,
        bytes32 dataHash
    );

    event PreJoinedLottery(
        bytes16 indexed thingId,
        address indexed user,
        bytes32 dataHash
    );

    event JoinedLottery(
        bytes16 indexed thingId,
        address indexed user,
        uint256 nonce
    );

    event LotteryClosedWithSuccess(
        bytes16 indexed thingId,
        address orchestrator,
        uint256 nonce,
        address[] winners
    );

    event LotteryClosedInFailure(bytes16 indexed thingId, address orchestrator);

    event SubLotteryInitiated(
        bytes16 indexed thingId,
        address orchestrator,
        bytes32 dataHash
    );

    event PreJoinedSubLottery(
        bytes16 indexed thingId,
        address indexed user,
        bytes32 dataHash
    );

    event JoinedSubLottery(
        bytes16 indexed thingId,
        address indexed user,
        uint256 nonce
    );

    event SubLotteryClosedWithSuccess(
        bytes16 indexed thingId,
        address orchestrator,
        uint256 nonce,
        address[] winners
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert ThingSubmissionVerifierLottery__NotOrchestrator();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert ThingSubmissionVerifierLottery__NotTruQuest();
        }
        _;
    }

    modifier onlyAcceptancePoll() {
        if (msg.sender != address(s_acceptancePoll)) {
            revert ThingSubmissionVerifierLottery__NotAcceptancePoll();
        }
        _;
    }

    modifier whenHasAtLeast(uint256 _requiredFunds) {
        if (!i_truQuest.checkHasAtLeast(msg.sender, _requiredFunds)) {
            revert ThingSubmissionVerifierLottery__NotEnoughFunds(
                _requiredFunds
            );
        }
        _;
    }

    modifier onlyOncePerLottery(bytes16 _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert ThingSubmissionVerifierLottery__AlreadyCommittedToLottery(
                _thingId
            );
        }
        _;
    }

    modifier onlyWhenLotteryActiveAndNotExpired(
        bytes16 _thingId,
        uint8 _margin
    ) {
        int64 lotteryInitBlock = s_thingIdToLotteryCommitments[_thingId][
            s_orchestrator
        ].block;
        if (lotteryInitBlock < 1) {
            revert ThingSubmissionVerifierLottery__LotteryNotActive(_thingId);
        }
        if (
            block.number + _margin > uint64(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingSubmissionVerifierLottery__LotteryExpired(_thingId);
        }
        _;
    }

    modifier onlyWhenLotteryActive(bytes16 _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block < 1) {
            revert ThingSubmissionVerifierLottery__LotteryNotActive(_thingId);
        }
        _;
    }

    modifier onlyOncePerSubLottery(bytes16 _thingId) {
        if (s_thingIdToSubLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert ThingSubmissionVerifierLottery__AlreadyCommittedToSubLottery(
                _thingId
            );
        }
        _;
    }

    modifier onlyWhenSubLotteryActiveAndNotExpired(
        bytes16 _thingId,
        uint8 _margin
    ) {
        int64 subLotteryInitBlock = s_thingIdToSubLotteryCommitments[_thingId][
            address(s_acceptancePoll)
        ].block;
        if (subLotteryInitBlock < 1) {
            revert ThingSubmissionVerifierLottery__SubLotteryNotActive(
                _thingId
            );
        }
        if (
            block.number + _margin >
            uint64(subLotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingSubmissionVerifierLottery__SubLotteryExpired(_thingId);
        }
        _;
    }

    modifier onlyWhenSubLotteryActive(bytes16 _thingId) {
        if (
            s_thingIdToSubLotteryCommitments[_thingId][
                address(s_acceptancePoll)
            ].block < 1
        ) {
            revert ThingSubmissionVerifierLottery__SubLotteryNotActive(
                _thingId
            );
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint8 _numVerifiers,
        uint256 _verifierStake,
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_numVerifiers = _numVerifiers;
        s_verifierStake = _verifierStake;
        s_durationBlocks = _durationBlocks;
    }

    function connectToAcceptancePoll(
        address _acceptancePollAddress
    ) external onlyTruQuest {
        s_acceptancePoll = AcceptancePoll(_acceptancePollAddress);
    }

    function computeHash(bytes32 _data) public view returns (bytes32) {
        return keccak256(abi.encodePacked(address(this), _data));
    }

    // onlyFunded
    function initLottery(
        bytes16 _thingId,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyOncePerLottery(_thingId) {
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = Commitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );

        emit LotteryInitiated(_thingId, s_orchestrator, _dataHash);
    }

    function preJoinLottery(
        bytes16 _thingId,
        bytes32 _dataHash
    )
        public
        onlyWhenLotteryActiveAndNotExpired(_thingId, 1)
        whenHasAtLeast(s_verifierStake)
        onlyOncePerLottery(_thingId)
    {
        i_truQuest.stake(msg.sender, s_verifierStake);
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = Commitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );
        s_participants[_thingId].push(msg.sender);

        emit PreJoinedLottery(_thingId, msg.sender, _dataHash);
    }

    function joinLottery(
        bytes16 _thingId,
        bytes32 _data
    ) public onlyWhenLotteryActiveAndNotExpired(_thingId, 0) {
        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (commitment.block == 0) {
            revert ThingSubmissionVerifierLottery__NotCommittedToLottery(
                _thingId
            );
        }
        if (commitment.revealed) {
            revert ThingSubmissionVerifierLottery__AlreadyJoinedLottery(
                _thingId
            );
        }
        s_thingIdToLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
                _thingId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        emit JoinedLottery(_thingId, msg.sender, nonce);
    }

    function computeNonce(
        bytes16 _thingId,
        bytes32 _data
    ) public view returns (uint256) {
        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];
        return
            uint256(
                keccak256(
                    abi.encodePacked(blockhash(uint64(commitment.block)), _data)
                )
            ) % MAX_NONCE;
    }

    function closeLotteryWithSuccess(
        bytes16 _thingId,
        bytes32 _data,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingId) {
        if (_winnerIndices.length != s_numVerifiers) {
            revert ThingSubmissionVerifierLottery__InvalidNumberOfLotteryWinners(
                s_numVerifiers,
                _winnerIndices.length
            );
        }

        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__InitAndCloseLotteryInTheSameBlock(
                _thingId
            );
        }

        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        uint64 j = 0;
        address[] memory participants = s_participants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint8 i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = participants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(participants[j], s_verifierStake);
            }
            ++j;
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        s_acceptancePoll.initPoll(_thingId, winners);

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        emit LotteryClosedWithSuccess(_thingId, s_orchestrator, nonce, winners);
    }

    // when not enough participants
    // @@??: add reason string/enum param ?
    function closeLotteryInFailure(
        bytes16 _thingId
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingId) {
        // checks?
        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        address[] memory participants = s_participants[_thingId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstake(participants[i], s_verifierStake);
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        emit LotteryClosedInFailure(_thingId, s_orchestrator);
    }

    function initSubLottery(
        bytes16 _thingId,
        bytes32 _dataHash
    ) external onlyAcceptancePoll onlyOncePerSubLottery(_thingId) {
        s_thingIdToSubLotteryCommitments[_thingId][msg.sender] = Commitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );

        emit SubLotteryInitiated(_thingId, s_orchestrator, _dataHash);
    }

    function preJoinSubLottery(
        bytes16 _thingId,
        bytes32 _dataHash
    )
        public
        onlyWhenSubLotteryActiveAndNotExpired(_thingId, 1)
        whenHasAtLeast(s_verifierStake)
        onlyOncePerSubLottery(_thingId)
    {
        i_truQuest.stake(msg.sender, s_verifierStake);
        s_thingIdToSubLotteryCommitments[_thingId][msg.sender] = Commitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );
        s_participants[_thingId].push(msg.sender);

        emit PreJoinedSubLottery(_thingId, msg.sender, _dataHash);
    }

    function joinSubLottery(
        bytes16 _thingId,
        bytes32 _data
    ) public onlyWhenSubLotteryActiveAndNotExpired(_thingId, 0) {
        Commitment memory commitment = s_thingIdToSubLotteryCommitments[
            _thingId
        ][msg.sender];

        if (commitment.block == 0) {
            revert ThingSubmissionVerifierLottery__NotCommittedToSubLottery(
                _thingId
            );
        }
        if (commitment.revealed) {
            revert ThingSubmissionVerifierLottery__AlreadyJoinedSubLottery(
                _thingId
            );
        }
        s_thingIdToSubLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidSubLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
                _thingId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        emit JoinedSubLottery(_thingId, msg.sender, nonce);
    }

    function closeSubLotteryWithSuccess(
        bytes16 _thingId,
        bytes32 _data,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenSubLotteryActive(_thingId) {
        uint8 numSubstituteVerifiers = s_numVerifiers -
            uint8(s_acceptancePoll.getVerifierCount(_thingId));
        if (_winnerIndices.length != numSubstituteVerifiers) {
            revert ThingSubmissionVerifierLottery__InvalidNumberOfLotteryWinners(
                numSubstituteVerifiers,
                _winnerIndices.length
            );
        }

        Commitment memory commitment = s_thingIdToSubLotteryCommitments[
            _thingId
        ][address(s_acceptancePoll)];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidSubLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
                _thingId
            );
        }

        s_thingIdToSubLotteryCommitments[_thingId][address(s_acceptancePoll)]
            .block = -1;

        uint64 j = 0;
        address[] memory participants = s_participants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint8 i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = participants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(participants[j], s_verifierStake);
            }
            ++j;
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        s_acceptancePoll.initSubPoll(_thingId, winners);

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        emit SubLotteryClosedWithSuccess(
            _thingId,
            s_orchestrator,
            nonce,
            winners
        );
    }
}
