// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./AssessmentPoll.sol";

error ThingAssessmentVerifierLottery__Unauthorized();
error ThingAssessmentVerifierLottery__AlreadyCommitted(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__NotActive(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__Expired(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__NotEnoughFunds();
error ThingAssessmentVerifierLottery__NotCommitted(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__AlreadyJoined(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__InvalidReveal(bytes32 thingProposalId);
error ThingAssessmentVerifierLottery__PreJoinAndJoinInTheSameBlock(
    bytes32 thingProposalId
);
error ThingAssessmentVerifierLottery__InvalidNumberOfWinners();
error ThingAssessmentVerifierLottery__InitAndCloseInTheSameBlock(
    bytes32 thingProposalId
);

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
        bytes16 indexed settlementProposalId,
        uint8 requiredNumVerifiers,
        uint8 joinedNumVerifiers
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert ThingAssessmentVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert ThingAssessmentVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlyAssessmentPoll() {
        if (msg.sender != address(s_assessmentPoll)) {
            revert ThingAssessmentVerifierLottery__Unauthorized();
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
            revert ThingAssessmentVerifierLottery__AlreadyCommitted(
                _thingProposalId
            );
        }
        _;
    }

    modifier onlyWhenActiveAndNotExpired(
        bytes32 _thingProposalId,
        uint8 _margin
    ) {
        int64 lotteryInitBlock = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][s_orchestrator].block;
        if (lotteryInitBlock < 1) {
            revert ThingAssessmentVerifierLottery__NotActive(_thingProposalId);
        }
        if (
            block.number + _margin > uint64(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingAssessmentVerifierLottery__Expired(_thingProposalId);
        }
        _;
    }

    modifier onlyWhenActive(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToLotteryCommitments[_thingProposalId][
                s_orchestrator
            ].block < 1
        ) {
            revert ThingAssessmentVerifierLottery__NotActive(_thingProposalId);
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
        onlyWhenActiveAndNotExpired(_thingProposalId, 0)
        whenHasEnoughFundsToStakeAsVerifier
        onlyOncePerLottery(_thingProposalId)
    {
        // @@??: Should I check here that msg.sender is one of the thing's verifiers or
        // just make a client-side check and then filter out invalid claims on the backend?
        // Logically, it makes sense to check, but it would mean higher txn cost for users.
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
        onlyWhenActiveAndNotExpired(_thingProposalId, 1)
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
    ) public onlyWhenActiveAndNotExpired(_thingProposalId, 0) {
        Commitment memory commitment = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][msg.sender];

        if (commitment.block == 0) {
            revert ThingAssessmentVerifierLottery__NotCommitted(
                _thingProposalId
            );
        }
        if (commitment.revealed) {
            revert ThingAssessmentVerifierLottery__AlreadyJoined(
                _thingProposalId
            );
        }
        s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
            .revealed = true;

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidReveal(
                _thingProposalId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__PreJoinAndJoinInTheSameBlock(
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
        for (; j < claimants.length; ++j) {
            i_truQuest.unstakeAsVerifier(claimants[j]);
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
        for (; j < participants.length; ++j) {
            i_truQuest.unstakeAsVerifier(participants[j]);
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
    ) public onlyOrchestrator onlyWhenActive(_thingProposalId) {
        if (
            _winnerClaimantIndices.length + _winnerIndices.length !=
            s_numVerifiers
        ) {
            revert ThingAssessmentVerifierLottery__InvalidNumberOfWinners();
        }

        Commitment memory commitment = s_thingProposalIdToLotteryCommitments[
            _thingProposalId
        ][msg.sender];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingAssessmentVerifierLottery__InvalidReveal(
                _thingProposalId
            );
        }

        uint64 commitmentBlock = uint64(commitment.block);
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__InitAndCloseInTheSameBlock(
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

    function closeLotteryInFailure(
        bytes32 _thingProposalId,
        uint8 _joinedNumVerifiers
    ) public onlyOrchestrator onlyWhenActive(_thingProposalId) {
        uint64 commitmentBlock = uint64(
            s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
                .block
        );
        if (uint64(block.number) == commitmentBlock) {
            revert ThingAssessmentVerifierLottery__InitAndCloseInTheSameBlock(
                _thingProposalId
            );
        }
        s_thingProposalIdToLotteryCommitments[_thingProposalId][msg.sender]
            .block = -1;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeProposalSubmitter(submitter);

        address[] memory claimants = s_claimants[_thingProposalId];
        for (uint64 i = 0; i < claimants.length; ++i) {
            i_truQuest.unstakeAsVerifier(claimants[i]);
        }

        address[] memory participants = s_participants[_thingProposalId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstakeAsVerifier(participants[i]);
        }

        s_claimants[_thingProposalId] = new address[](0); // unnecessary?
        delete s_claimants[_thingProposalId];

        s_participants[_thingProposalId] = new address[](0); // unnecessary?
        delete s_participants[_thingProposalId];

        emit LotteryClosedInFailure(
            thingId,
            proposalId,
            s_numVerifiers,
            _joinedNumVerifiers
        );
    }
}
