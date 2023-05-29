// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./AcceptancePoll.sol";

error ThingSubmissionVerifierLottery__Unauthorized();
error ThingSubmissionVerifierLottery__AlreadyCommitted(bytes16 thingId);
error ThingSubmissionVerifierLottery__NotActive(bytes16 thingId);
error ThingSubmissionVerifierLottery__Expired(bytes16 thingId);
error ThingSubmissionVerifierLottery__NotEnoughFunds();
error ThingSubmissionVerifierLottery__NotCommitted(bytes16 thingId);
error ThingSubmissionVerifierLottery__AlreadyJoined(bytes16 thingId);
error ThingSubmissionVerifierLottery__InvalidReveal(bytes16 thingId);
error ThingSubmissionVerifierLottery__PreJoinAndJoinInTheSameBlock(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__InvalidNumberOfWinners();
error ThingSubmissionVerifierLottery__InitAndCloseInTheSameBlock(
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
    uint16 private s_durationBlocks;

    mapping(bytes16 => mapping(address => Commitment))
        private s_thingIdToLotteryCommitments;
    mapping(bytes16 => address[]) private s_participants;

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

    event LotteryClosedInFailure(bytes16 indexed thingId);

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert ThingSubmissionVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert ThingSubmissionVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlyAcceptancePoll() {
        if (msg.sender != address(s_acceptancePoll)) {
            revert ThingSubmissionVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier whenHasEnoughFundsToStakeAsVerifier() {
        if (!i_truQuest.checkHasEnoughFundsToStakeAsVerifier(msg.sender)) {
            revert ThingSubmissionVerifierLottery__NotEnoughFunds();
        }
        _;
    }

    modifier onlyOncePerLottery(bytes16 _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert ThingSubmissionVerifierLottery__AlreadyCommitted(_thingId);
        }
        _;
    }

    modifier onlyWhenActiveAndNotExpired(bytes16 _thingId, uint8 _margin) {
        int64 lotteryInitBlock = s_thingIdToLotteryCommitments[_thingId][
            s_orchestrator
        ].block;
        if (lotteryInitBlock < 1) {
            revert ThingSubmissionVerifierLottery__NotActive(_thingId);
        }
        if (
            block.number + _margin > uint64(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingSubmissionVerifierLottery__Expired(_thingId);
        }
        _;
    }

    modifier onlyWhenActive(bytes16 _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block < 1) {
            revert ThingSubmissionVerifierLottery__NotActive(_thingId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint8 _numVerifiers,
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_numVerifiers = _numVerifiers;
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

    function getLotteryDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getLotteryInitBlock(bytes16 _thingId) public view returns (int64) {
        return s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block;
    }

    function checkAlreadyPreJoinedLottery(
        bytes16 _thingId,
        address _user
    ) public view returns (bool) {
        return s_thingIdToLotteryCommitments[_thingId][_user].block != 0;
    }

    function checkAlreadyJoinedLottery(
        bytes16 _thingId,
        address _user
    ) public view returns (bool) {
        return s_thingIdToLotteryCommitments[_thingId][_user].revealed;
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
        onlyWhenActiveAndNotExpired(_thingId, 1)
        whenHasEnoughFundsToStakeAsVerifier
        onlyOncePerLottery(_thingId)
    {
        i_truQuest.stakeAsVerifier(msg.sender);
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
    ) public onlyWhenActiveAndNotExpired(_thingId, 0) {
        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (commitment.block == 0) {
            revert ThingSubmissionVerifierLottery__NotCommitted(_thingId);
        }
        if (commitment.revealed) {
            revert ThingSubmissionVerifierLottery__AlreadyJoined(_thingId);
        }
        s_thingIdToLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__PreJoinAndJoinInTheSameBlock(
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
        address _user,
        bytes32 _data
    ) public view returns (uint256) {
        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            _user
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
    ) public onlyOrchestrator onlyWhenActive(_thingId) {
        if (_winnerIndices.length != s_numVerifiers) {
            revert ThingSubmissionVerifierLottery__InvalidNumberOfWinners();
        }

        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidReveal(_thingId);
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingSubmissionVerifierLottery__InitAndCloseInTheSameBlock(
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
                i_truQuest.unstakeAsVerifier(participants[j]);
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
    ) public onlyOrchestrator onlyWhenActive(_thingId) {
        // checks?
        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        address[] memory participants = s_participants[_thingId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstakeAsVerifier(participants[i]);
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        emit LotteryClosedInFailure(_thingId);
    }
}
