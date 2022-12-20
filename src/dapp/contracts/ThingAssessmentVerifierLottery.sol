// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";

error ThingAssessmentVerifierLottery__NotOrchestrator();
error ThingAssessmentVerifierLottery__NotTruQuest();
// error ThingAssessmentVerifierLottery__NotAcceptancePoll();
error ThingAssessmentVerifierLottery__AlreadyCommittedToLottery(string thingId);
error ThingAssessmentVerifierLottery__LotteryNotActive(string thingId);
error ThingAssessmentVerifierLottery__LotteryExpired(string thingId);
error ThingAssessmentVerifierLottery__NotEnoughFunds(uint256 requiredFunds);
error ThingAssessmentVerifierLottery__NotCommittedToLottery(string thingId);
error ThingAssessmentVerifierLottery__AlreadyJoinedLottery(string thingId);
error ThingAssessmentVerifierLottery__InvalidLotteryReveal(string thingId);
error ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
    string thingId
);
error ThingAssessmentVerifierLottery__InvalidNumberOfLotteryWinners();
error ThingAssessmentVerifierLottery__InitAndCloseLotteryInTheSameBlock(
    string thingId
);

// error ThingAssessmentVerifierLottery__SubLotteryNotActive(string thingId);
// error ThingAssessmentVerifierLottery__SubLotteryExpired(string thingId);
// error ThingAssessmentVerifierLottery__AlreadyCommittedToSubLottery(
//     string thingId
// );
// error ThingAssessmentVerifierLottery__NotCommittedToSubLottery(string thingId);
// error ThingAssessmentVerifierLottery__AlreadyJoinedSubLottery(string thingId);
// error ThingAssessmentVerifierLottery__InvalidSubLotteryReveal(string thingId);
// error ThingAssessmentVerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
//     string thingId
// );
// error ThingAssessmentVerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
//     string thingId
// );

contract ThingAssessmentVerifierLottery {
    struct Commitment {
        bytes32 dataHash;
        int64 block;
        bool revealed;
    }

    uint256 public constant MAX_NONCE = 1000000;

    TruQuest private immutable i_truQuest;
    // AcceptancePoll private s_assessmentPoll;
    address private s_orchestrator;

    uint8 private s_numVerifiers;
    uint256 private s_verifierStake;
    uint16 private s_durationBlocks;

    mapping(string => mapping(address => Commitment))
        private s_thingIdToLotteryCommitments;
    mapping(string => address[]) private s_participants;
    mapping(string => address[]) private s_claimants;
    // mapping(string => mapping(address => Commitment))
    //     private s_thingIdToSubLotteryCommitments;

    event LotteryInitiated(
        string indexed thingId,
        string indexed settlementProposalId,
        address orchestrator,
        bytes32 dataHash
    );

    event LotterySpotClaimed(
        string indexed thingId,
        string indexed settlementProposalId,
        address indexed user
    );

    event PreJoinedLottery(
        string indexed thingId,
        string indexed settlementProposalId,
        address indexed user,
        bytes32 dataHash
    );

    event JoinedLottery(
        string indexed thingId,
        string indexed settlementProposalId,
        address indexed user,
        uint256 nonce
    );

    event LotteryClosedWithSuccess(
        string indexed thingId,
        string indexed settlementProposalId,
        address orchestrator,
        uint256 nonce,
        address[] claimants,
        address[] winners
    );

    event LotteryClosedInFailure(
        string indexed thingId,
        string indexed settlementProposalId,
        address orchestrator
    );

    // event SubLotteryInitiated(
    //     string indexed thingId,
    //     address orchestrator,
    //     bytes32 dataHash
    // );

    // event PreJoinedSubLottery(
    //     string indexed thingId,
    //     address indexed user,
    //     bytes32 dataHash
    // );

    // event JoinedSubLottery(
    //     string indexed thingId,
    //     address indexed user,
    //     uint256 nonce
    // );

    // event SubLotteryClosedWithSuccess(
    //     string indexed thingId,
    //     address orchestrator,
    //     uint256 nonce,
    //     address[] winners
    // );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert ThingAssessmentVerifierLottery__NotOrchestrator();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert ThingAssessmentVerifierLottery__NotTruQuest();
        }
        _;
    }

    // modifier onlyAcceptancePoll() {
    //     if (msg.sender != address(s_assessmentPoll)) {
    //         revert ThingAssessmentVerifierLottery__NotAcceptancePoll();
    //     }
    //     _;
    // }

    modifier whenHasAtLeast(uint256 _requiredFunds) {
        if (!i_truQuest.checkHasAtLeast(msg.sender, _requiredFunds)) {
            revert ThingAssessmentVerifierLottery__NotEnoughFunds(
                _requiredFunds
            );
        }
        _;
    }

    modifier onlyOncePerLottery(string calldata _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][msg.sender].block != 0) {
            revert ThingAssessmentVerifierLottery__AlreadyCommittedToLottery(
                _thingId
            );
        }
        _;
    }

    modifier onlyWhenLotteryActiveAndNotExpired(
        string calldata _thingId,
        uint8 _margin
    ) {
        int64 lotteryInitBlock = s_thingIdToLotteryCommitments[_thingId][
            s_orchestrator
        ].block;
        if (lotteryInitBlock < 1) {
            revert ThingAssessmentVerifierLottery__LotteryNotActive(_thingId);
        }
        if (
            block.number + _margin > uint64(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingAssessmentVerifierLottery__LotteryExpired(_thingId);
        }
        _;
    }

    modifier onlyWhenLotteryActive(string calldata _thingId) {
        if (s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block < 1) {
            revert ThingAssessmentVerifierLottery__LotteryNotActive(_thingId);
        }
        _;
    }

    // modifier onlyOncePerSubLottery(string calldata _thingId) {
    //     if (s_thingIdToSubLotteryCommitments[_thingId][msg.sender].block != 0) {
    //         revert ThingAssessmentVerifierLottery__AlreadyCommittedToSubLottery(
    //             _thingId
    //         );
    //     }
    //     _;
    // }

    // modifier onlyWhenSubLotteryActiveAndNotExpired(
    //     string calldata _thingId,
    //     uint8 _margin
    // ) {
    //     int64 subLotteryInitBlock = s_thingIdToSubLotteryCommitments[_thingId][
    //         address(s_assessmentPoll)
    //     ].block;
    //     if (subLotteryInitBlock < 1) {
    //         revert ThingAssessmentVerifierLottery__SubLotteryNotActive(
    //             _thingId
    //         );
    //     }
    //     if (
    //         block.number + _margin >
    //         uint64(subLotteryInitBlock) + s_durationBlocks
    //     ) {
    //         revert ThingAssessmentVerifierLottery__SubLotteryExpired(_thingId);
    //     }
    //     _;
    // }

    // modifier onlyWhenSubLotteryActive(string calldata _thingId) {
    //     if (
    //         s_thingIdToSubLotteryCommitments[_thingId][
    //             address(s_assessmentPoll)
    //         ].block < 1
    //     ) {
    //         revert ThingAssessmentVerifierLottery__SubLotteryNotActive(
    //             _thingId
    //         );
    //     }
    //     _;
    // }

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

    // function connectToAcceptancePoll(
    //     address _acceptancePollAddress
    // ) external onlyTruQuest {
    //     s_assessmentPoll = AcceptancePoll(_acceptancePollAddress);
    // }

    function computeHash(bytes32 _data) public view returns (bytes32) {
        return keccak256(abi.encodePacked(address(this), _data));
    }

    function initLottery(
        string calldata _thingId,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyOncePerLottery(_thingId) {
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = Commitment(
            _dataHash,
            int64(uint64(block.number)),
            false
        );

        string memory settlementProposalId = i_truQuest.getSettlementProposalId(
            _thingId
        );

        // onlyFunded
        // if (bytes(settlementProposalId).length == 0) {
        //     // ... not funded
        // }

        emit LotteryInitiated(
            _thingId,
            settlementProposalId,
            s_orchestrator,
            _dataHash
        );
    }

    function claimLotterySpot(
        string calldata _thingId
    )
        public
        onlyWhenLotteryActiveAndNotExpired(_thingId, 0)
        whenHasAtLeast(s_verifierStake)
        onlyOncePerLottery(_thingId)
    {
        i_truQuest.stake(msg.sender, s_verifierStake);
        s_thingIdToLotteryCommitments[_thingId][msg.sender] = Commitment(
            bytes32(0),
            int64(uint64(block.number)),
            true
        );
        s_claimants[_thingId].push(msg.sender);

        string memory settlementProposalId = i_truQuest.getSettlementProposalId(
            _thingId
        );

        emit LotterySpotClaimed(_thingId, settlementProposalId, msg.sender);
    }

    function preJoinLottery(
        string calldata _thingId,
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

        string memory settlementProposalId = i_truQuest.getSettlementProposalId(
            _thingId
        );

        emit PreJoinedLottery(
            _thingId,
            settlementProposalId,
            msg.sender,
            _dataHash
        );
    }

    function joinLottery(
        string calldata _thingId,
        bytes32 _data
    ) public onlyWhenLotteryActiveAndNotExpired(_thingId, 0) {
        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (commitment.block == 0) {
            revert ThingAssessmentVerifierLottery__NotCommittedToLottery(
                _thingId
            );
        }
        if (commitment.revealed) {
            revert ThingAssessmentVerifierLottery__AlreadyJoinedLottery(
                _thingId
            );
        }
        s_thingIdToLotteryCommitments[_thingId][msg.sender].revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
                _thingId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        string memory settlementProposalId = i_truQuest.getSettlementProposalId(
            _thingId
        );

        emit JoinedLottery(_thingId, settlementProposalId, msg.sender, nonce);
    }

    function computeNonce(
        string calldata _thingId,
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

    function _getWinnerClaimants(
        string calldata _thingId,
        uint64[] calldata _winnerClaimantIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory claimants = s_claimants[_thingId];
        address[] memory winners = new address[](_winnerClaimantIndices.length);
        for (uint i = 0; i < _winnerClaimantIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerClaimantIndices[i];
            winners[i] = claimants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(claimants[j], s_verifierStake);
            }
            ++j;
        }

        s_claimants[_thingId] = new address[](0); // unnecessary?
        delete s_claimants[_thingId];

        return winners;
    }

    function _getLotteryWinners(
        string calldata _thingId,
        uint64[] calldata _winnerIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory participants = s_participants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = participants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstake(participants[j], s_verifierStake);
            }
            ++j;
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        return winners;
    }

    function closeLotteryWithSuccess(
        string calldata _thingId,
        bytes32 _data,
        uint64[] calldata _winnerClaimantIndices,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingId) {
        if (
            _winnerClaimantIndices.length + _winnerIndices.length !=
            s_numVerifiers
        ) {
            revert ThingAssessmentVerifierLottery__InvalidNumberOfLotteryWinners();
        }

        Commitment memory commitment = s_thingIdToLotteryCommitments[_thingId][
            msg.sender
        ];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidLotteryReveal(
                _thingId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__InitAndCloseLotteryInTheSameBlock(
                _thingId
            );
        }

        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        address[] memory claimants = _getWinnerClaimants(
            _thingId,
            _winnerClaimantIndices
        );

        address[] memory winners = _getLotteryWinners(_thingId, _winnerIndices);

        // s_assessmentPoll.initPoll(_thingId, winners);

        uint256 nonce = uint256(
            keccak256(abi.encodePacked(blockhash(commitmentBlock), _data))
        ) % MAX_NONCE;

        emit LotteryClosedWithSuccess(
            _thingId,
            i_truQuest.getSettlementProposalId(_thingId),
            s_orchestrator,
            nonce,
            claimants,
            winners
        );
    }

    // when not enough participants
    // @@??: add reason string/enum param ?
    function closeLotteryInFailure(
        string calldata _thingId
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingId) {
        // checks?
        s_thingIdToLotteryCommitments[_thingId][msg.sender].block = -1;

        address[] memory participants = s_participants[_thingId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstake(participants[i], s_verifierStake);
        }

        s_participants[_thingId] = new address[](0); // unnecessary?
        delete s_participants[_thingId];

        string memory settlementProposalId = i_truQuest.getSettlementProposalId(
            _thingId
        );

        emit LotteryClosedInFailure(
            _thingId,
            settlementProposalId,
            s_orchestrator
        );
    }

    // function initSubLottery(
    //     string calldata _thingId,
    //     bytes32 _dataHash
    // ) external onlyAcceptancePoll onlyOncePerSubLottery(_thingId) {
    //     s_thingIdToSubLotteryCommitments[_thingId][msg.sender] = Commitment(
    //         _dataHash,
    //         int64(uint64(block.number)),
    //         false
    //     );

    //     emit SubLotteryInitiated(_thingId, s_orchestrator, _dataHash);
    // }

    // function preJoinSubLottery(
    //     string calldata _thingId,
    //     bytes32 _dataHash
    // )
    //     public
    //     onlyWhenSubLotteryActiveAndNotExpired(_thingId, 1)
    //     whenHasAtLeast(s_verifierStake)
    //     onlyOncePerSubLottery(_thingId)
    // {
    //     i_truQuest.stake(msg.sender, s_verifierStake);
    //     s_thingIdToSubLotteryCommitments[_thingId][msg.sender] = Commitment(
    //         _dataHash,
    //         int64(uint64(block.number)),
    //         false
    //     );
    //     s_participants[_thingId].push(msg.sender);

    //     emit PreJoinedSubLottery(_thingId, msg.sender, _dataHash);
    // }

    // function joinSubLottery(
    //     string calldata _thingId,
    //     bytes32 _data
    // ) public onlyWhenSubLotteryActiveAndNotExpired(_thingId, 0) {
    //     Commitment memory commitment = s_thingIdToSubLotteryCommitments[
    //         _thingId
    //     ][msg.sender];

    //     if (commitment.block == 0) {
    //         revert ThingAssessmentVerifierLottery__NotCommittedToSubLottery(
    //             _thingId
    //         );
    //     }
    //     if (commitment.revealed) {
    //         revert ThingAssessmentVerifierLottery__AlreadyJoinedSubLottery(
    //             _thingId
    //         );
    //     }
    //     s_thingIdToSubLotteryCommitments[_thingId][msg.sender].revealed = true;

    //     if (computeHash(_data) != commitment.dataHash) {
    //         revert ThingAssessmentVerifierLottery__InvalidSubLotteryReveal(
    //             _thingId
    //         );
    //     }

    //     uint64 commitmentBlock = uint64(commitment.block);
    //     if (uint64(block.number) == commitmentBlock) {
    //         revert ThingAssessmentVerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
    //             _thingId
    //         );
    //     }

    //     bytes32 blockHash = blockhash(commitmentBlock);
    //     uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
    //         MAX_NONCE;

    //     emit JoinedSubLottery(_thingId, msg.sender, nonce);
    // }

    // function closeSubLotteryWithSuccess(
    //     string calldata _thingId,
    //     bytes32 _data,
    //     uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    // ) public onlyOrchestrator onlyWhenSubLotteryActive(_thingId) {
    //     uint8 numSubstituteVerifiers = s_numVerifiers -
    //         uint8(s_assessmentPoll.getVerifierCount(_thingId));
    //     if (_winnerIndices.length != numSubstituteVerifiers) {
    //         revert ThingAssessmentVerifierLottery__InvalidNumberOfLotteryWinners(
    //             numSubstituteVerifiers,
    //             _winnerIndices.length
    //         );
    //     }

    //     Commitment memory commitment = s_thingIdToSubLotteryCommitments[
    //         _thingId
    //     ][address(s_assessmentPoll)];

    //     if (computeHash(_data) != commitment.dataHash) {
    //         revert ThingAssessmentVerifierLottery__InvalidSubLotteryReveal(
    //             _thingId
    //         );
    //     }

    //     uint64 commitmentBlock = uint64(commitment.block);
    //     if (uint64(block.number) == commitmentBlock) {
    //         revert ThingAssessmentVerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
    //             _thingId
    //         );
    //     }

    //     s_thingIdToSubLotteryCommitments[_thingId][address(s_assessmentPoll)]
    //         .block = -1;

    //     uint64 j = 0;
    //     address[] memory participants = s_participants[_thingId];
    //     address[] memory winners = new address[](_winnerIndices.length);
    //     for (uint8 i = 0; i < _winnerIndices.length; ++i) {
    //         uint64 nextWinnerIndex = _winnerIndices[i];
    //         winners[i] = participants[nextWinnerIndex];
    //         for (; j < nextWinnerIndex; ++j) {
    //             i_truQuest.unstake(participants[j], s_verifierStake);
    //         }
    //         ++j;
    //     }

    //     s_participants[_thingId] = new address[](0); // unnecessary?
    //     delete s_participants[_thingId];

    //     s_assessmentPoll.initSubPoll(_thingId, winners);

    //     bytes32 blockHash = blockhash(commitmentBlock);
    //     uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
    //         MAX_NONCE;

    //     emit SubLotteryClosedWithSuccess(
    //         _thingId,
    //         s_orchestrator,
    //         nonce,
    //         winners
    //     );
    // }

    function getLotteryDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getLotteryInitBlockNumber(
        string calldata _thingId
    ) public view returns (int64) {
        return s_thingIdToLotteryCommitments[_thingId][s_orchestrator].block;
    }
}
