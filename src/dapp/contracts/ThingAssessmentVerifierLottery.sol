// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./AssessmentPoll.sol";

error ThingAssessmentVerifierLottery__NotOrchestrator();
error ThingAssessmentVerifierLottery__NotTruQuest();
error ThingAssessmentVerifierLottery__NotAssessmentPoll();
error ThingAssessmentVerifierLottery__AlreadyCommittedToLottery(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__LotteryNotActive(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__LotteryExpired(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__NotEnoughFunds();
error ThingAssessmentVerifierLottery__NotCommittedToLottery(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__AlreadyJoinedLottery(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__InvalidLotteryReveal(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__InvalidNumberOfLotteryWinners();
error ThingAssessmentVerifierLottery__InitAndCloseLotteryInTheSameBlock(
    bytes32 thingProposalId
);

// error ThingAssessmentVerifierLottery__SubLotteryNotActive(bytes16 thingId);
// error ThingAssessmentVerifierLottery__SubLotteryExpired(bytes16 thingId);
// error ThingAssessmentVerifierLottery__AlreadyCommittedToSubLottery(
//     bytes16 thingId
// );
// error ThingAssessmentVerifierLottery__NotCommittedToSubLottery(bytes16 thingId);
// error ThingAssessmentVerifierLottery__AlreadyJoinedSubLottery(bytes16 thingId);
// error ThingAssessmentVerifierLottery__InvalidSubLotteryReveal(bytes16 thingId);
// error ThingAssessmentVerifierLottery__PreJoinAndJoinSubLotteryInTheSameBlock(
//     bytes16 thingId
// );
// error ThingAssessmentVerifierLottery__InitAndCloseSubLotteryInTheSameBlock(
//     bytes16 thingId
// );

contract ThingAssessmentVerifierLottery {
    struct Commitment {
        bytes32 dataHash;
        int64 block;
        bool revealed;
    }

    uint256 public constant MAX_NONCE = 1000000;

    TruQuest private immutable i_truQuest;
    AssessmentPoll private s_assessmentPoll;
    address private s_orchestrator;

    uint8 private s_numVerifiers;
    uint16 private s_durationBlocks;

    mapping(bytes32 => mapping(address => Commitment))
        private s_thingProposalIdToLotteryCommitments;
    mapping(bytes32 => address[]) private s_participants;
    mapping(bytes32 => address[]) private s_claimants;
    // mapping(bytes16 => mapping(address => Commitment))
    //     private s_thingIdToSubLotteryCommitments;

    event LotteryInitiated(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address orchestrator,
        bytes32 dataHash
    );

    event LotterySpotClaimed(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user
    );

    event PreJoinedLottery(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        bytes32 dataHash
    );

    event JoinedLottery(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        uint256 nonce
    );

    event LotteryClosedWithSuccess(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address orchestrator,
        uint256 nonce,
        address[] claimants,
        address[] winners
    );

    event LotteryClosedInFailure(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId
    );

    // event SubLotteryInitiated(
    //     bytes16 indexed thingId,
    //     bytes32 dataHash
    // );

    // event PreJoinedSubLottery(
    //     bytes16 indexed thingId,
    //     address indexed user,
    //     bytes32 dataHash
    // );

    // event JoinedSubLottery(
    //     bytes16 indexed thingId,
    //     address indexed user,
    //     uint256 nonce
    // );

    // event SubLotteryClosedWithSuccess(
    //     bytes16 indexed thingId,
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

    modifier onlyAssessmentPoll() {
        if (msg.sender != address(s_assessmentPoll)) {
            revert ThingAssessmentVerifierLottery__NotAssessmentPoll();
        }
        _;
    }

    modifier whenHasEnoughFundsToStakeAsVerifier() {
        if (!i_truQuest.checkHasEnoughFundsToStakeAsVerifier(msg.sender)) {
            revert ThingAssessmentVerifierLottery__NotEnoughFunds();
        }
        _;
    }

    modifier onlyOncePerLottery(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
                .block != 0
        ) {
            revert ThingAssessmentVerifierLottery__AlreadyCommittedToLottery(
                _thingProposalId
            );
        }
        _;
    }

    modifier onlyWhenLotteryActiveAndNotExpired(
        bytes32 _thingProposalId,
        uint8 _margin
    ) {
        int64 lotteryInitBlock = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][s_orchestrator].block;
        if (lotteryInitBlock < 1) {
            revert ThingAssessmentVerifierLottery__LotteryNotActive(
                _thingProposalId
            );
        }
        if (
            block.number + _margin > uint64(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingAssessmentVerifierLottery__LotteryExpired(
                _thingProposalId
            );
        }
        _;
    }

    modifier onlyWhenLotteryActive(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToLotteryCommitments[_thingProposalId][
                s_orchestrator
            ].block < 1
        ) {
            revert ThingAssessmentVerifierLottery__LotteryNotActive(
                _thingProposalId
            );
        }
        _;
    }

    // modifier onlyOncePerSubLottery(bytes16 _thingId) {
    //     if (s_thingIdToSubLotteryCommitments[_thingId][msg.sender].block != 0) {
    //         revert ThingAssessmentVerifierLottery__AlreadyCommittedToSubLottery(
    //             _thingId
    //         );
    //     }
    //     _;
    // }

    // modifier onlyWhenSubLotteryActiveAndNotExpired(
    //     bytes16 _thingId,
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

    // modifier onlyWhenSubLotteryActive(bytes16 _thingId) {
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
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_numVerifiers = _numVerifiers;
        s_durationBlocks = _durationBlocks;
    }

    function connectToAssessmentPoll(
        address _assessmentPollAddress
    ) external onlyTruQuest {
        s_assessmentPoll = AssessmentPoll(_assessmentPollAddress);
    }

    function computeHash(bytes32 _data) public view returns (bytes32) {
        return keccak256(abi.encodePacked(address(this), _data));
    }

    function getLotteryDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getLotteryInitBlock(
        bytes32 _thingProposalId
    ) public view returns (int64) {
        return
            s_thingProposalIdToLotteryCommitments[_thingProposalId][
                s_orchestrator
            ].block;
    }

    function checkAlreadyClaimedALotterySpot(
        bytes32 _thingProposalId,
        address _user
    ) public view returns (bool) {
        return
            s_thingProposalIdToLotteryCommitments[_thingProposalId][_user]
                .revealed &&
            s_thingProposalIdToLotteryCommitments[_thingProposalId][_user]
                .dataHash ==
            bytes32(0);
    }

    function checkAlreadyPreJoinedLottery(
        bytes32 _thingProposalId,
        address _user
    ) public view returns (bool) {
        return
            s_thingProposalIdToLotteryCommitments[_thingProposalId][_user]
                .block != 0;
    }

    function checkAlreadyJoinedLottery(
        bytes32 _thingProposalId,
        address _user
    ) public view returns (bool) {
        return
            s_thingProposalIdToLotteryCommitments[_thingProposalId][_user]
                .revealed;
    }

    function _splitIds(
        bytes32 _thingProposalId
    ) private pure returns (bytes16 _thingId, bytes16 _settlementProposalId) {
        _thingId = bytes16(_thingProposalId);
        _settlementProposalId = bytes16(uint128(uint256(_thingProposalId)));
    }

    function initLottery(
        bytes32 _thingProposalId,
        bytes32 _dataHash
    ) public onlyOrchestrator onlyOncePerLottery(_thingProposalId) {
        s_thingProposalIdToLotteryCommitments[_thingProposalId][
            msg.sender
        ] = Commitment(_dataHash, int64(uint64(block.number)), false);

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        // bytes16 currentProposalId = i_truQuest.getSettlementProposalId(
        //     thingId
        // );

        // if (proposalId != currentProposalId) {
        //     // ...
        // }

        emit LotteryInitiated(thingId, proposalId, s_orchestrator, _dataHash);
    }

    function claimLotterySpot(
        bytes32 _thingProposalId
    )
        public
        onlyWhenLotteryActiveAndNotExpired(_thingProposalId, 0)
        whenHasEnoughFundsToStakeAsVerifier
        onlyOncePerLottery(_thingProposalId)
    {
        i_truQuest.stakeAsVerifier(msg.sender);
        s_thingProposalIdToLotteryCommitments[_thingProposalId][
            msg.sender
        ] = Commitment(bytes32(0), int64(uint64(block.number)), true);
        s_claimants[_thingProposalId].push(msg.sender);

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        emit LotterySpotClaimed(thingId, proposalId, msg.sender);
    }

    function preJoinLottery(
        bytes32 _thingProposalId,
        bytes32 _dataHash
    )
        public
        onlyWhenLotteryActiveAndNotExpired(_thingProposalId, 1)
        whenHasEnoughFundsToStakeAsVerifier
        onlyOncePerLottery(_thingProposalId)
    {
        i_truQuest.stakeAsVerifier(msg.sender);
        s_thingProposalIdToLotteryCommitments[_thingProposalId][
            msg.sender
        ] = Commitment(_dataHash, int64(uint64(block.number)), false);
        s_participants[_thingProposalId].push(msg.sender);

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        emit PreJoinedLottery(thingId, proposalId, msg.sender, _dataHash);
    }

    function joinLottery(
        bytes32 _thingProposalId,
        bytes32 _data
    ) public onlyWhenLotteryActiveAndNotExpired(_thingProposalId, 0) {
        Commitment memory commitment = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][msg.sender];

        if (commitment.block == 0) {
            revert ThingAssessmentVerifierLottery__NotCommittedToLottery(
                _thingProposalId
            );
        }
        if (commitment.revealed) {
            revert ThingAssessmentVerifierLottery__AlreadyJoinedLottery(
                _thingProposalId
            );
        }
        s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
            .revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidLotteryReveal(
                _thingProposalId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__PreJoinAndJoinLotteryInTheSameBlock(
                _thingProposalId
            );
        }

        bytes32 blockHash = blockhash(commitmentBlock);
        uint256 nonce = uint256(keccak256(abi.encodePacked(blockHash, _data))) %
            MAX_NONCE;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        emit JoinedLottery(thingId, proposalId, msg.sender, nonce);
    }

    function computeNonce(
        bytes32 _thingProposalId,
        address _user,
        bytes32 _data
    ) public view returns (uint256) {
        Commitment memory commitment = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][_user];
        return
            uint256(
                keccak256(
                    abi.encodePacked(blockhash(uint64(commitment.block)), _data)
                )
            ) % MAX_NONCE;
    }

    function _getWinnerClaimants(
        bytes32 _thingProposalId,
        uint64[] calldata _winnerClaimantIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory claimants = s_claimants[_thingProposalId];
        address[] memory winners = new address[](_winnerClaimantIndices.length);
        for (uint i = 0; i < _winnerClaimantIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerClaimantIndices[i];
            winners[i] = claimants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstakeAsVerifier(claimants[j]);
            }
            ++j;
        }

        s_claimants[_thingProposalId] = new address[](0); // unnecessary?
        delete s_claimants[_thingProposalId];

        return winners;
    }

    function _getLotteryWinners(
        bytes32 _thingProposalId,
        uint64[] calldata _winnerIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory participants = s_participants[_thingProposalId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint i = 0; i < _winnerIndices.length; ++i) {
            uint64 nextWinnerIndex = _winnerIndices[i];
            winners[i] = participants[nextWinnerIndex];
            for (; j < nextWinnerIndex; ++j) {
                i_truQuest.unstakeAsVerifier(participants[j]);
            }
            ++j;
        }

        s_participants[_thingProposalId] = new address[](0); // unnecessary?
        delete s_participants[_thingProposalId];

        return winners;
    }

    function _concatWinners(
        address[] memory _claimants,
        address[] memory _winners
    ) private pure returns (address[] memory _verifiers) {
        _verifiers = new address[](_claimants.length + _winners.length);
        uint j = 0;
        for (uint i = 0; i < _claimants.length; ) {
            _verifiers[j++] = _claimants[i++];
        }
        for (uint i = 0; i < _winners.length; ) {
            _verifiers[j++] = _winners[i++];
        }
    }

    function closeLotteryWithSuccess(
        bytes32 _thingProposalId,
        bytes32 _data,
        uint64[] calldata _winnerClaimantIndices,
        uint64[] calldata _winnerIndices // sorted asc indices of users in prejoin array
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingProposalId) {
        if (
            _winnerClaimantIndices.length + _winnerIndices.length !=
            s_numVerifiers
        ) {
            revert ThingAssessmentVerifierLottery__InvalidNumberOfLotteryWinners();
        }

        Commitment memory commitment = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][msg.sender];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidLotteryReveal(
                _thingProposalId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__InitAndCloseLotteryInTheSameBlock(
                _thingProposalId
            );
        }

        s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
            .block = -1;

        address[] memory claimants = _getWinnerClaimants(
            _thingProposalId,
            _winnerClaimantIndices
        );

        address[] memory winners = _getLotteryWinners(
            _thingProposalId,
            _winnerIndices
        );

        address[] memory verifiers = _concatWinners(claimants, winners);

        s_assessmentPoll.initPoll(_thingProposalId, verifiers);

        uint256 nonce = uint256(
            keccak256(abi.encodePacked(blockhash(commitmentBlock), _data))
        ) % MAX_NONCE;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        emit LotteryClosedWithSuccess(
            thingId,
            proposalId,
            s_orchestrator,
            nonce,
            claimants,
            winners
        );
    }

    // when not enough participants
    // @@??: add reason string/enum param ?
    function closeLotteryInFailure(
        bytes32 _thingProposalId
    ) public onlyOrchestrator onlyWhenLotteryActive(_thingProposalId) {
        // checks?
        s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
            .block = -1;

        // @@TODO: unstake submitter with slashing?

        address[] memory participants = s_participants[_thingProposalId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstakeAsVerifier(participants[i]);
        }

        s_participants[_thingProposalId] = new address[](0); // unnecessary?
        delete s_participants[_thingProposalId];

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        emit LotteryClosedInFailure(thingId, proposalId);
    }

    // function initSubLottery(
    //     bytes16 _thingId,
    //     bytes32 _dataHash
    // ) external onlyAcceptancePoll onlyOncePerSubLottery(_thingId) {
    //     s_thingIdToSubLotteryCommitments[_thingId][msg.sender] = Commitment(
    //         _dataHash,
    //         int64(uint64(block.number)),
    //         false
    //     );

    //     emit SubLotteryInitiated(_thingId, _dataHash);
    // }

    // function preJoinSubLottery(
    //     bytes16 _thingId,
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
    //     bytes16 _thingId,
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
    //     bytes16 _thingId,
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
    //         nonce,
    //         winners
    //     );
    // }
}
