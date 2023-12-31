// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingValidationPoll.sol";
import "./SettlementProposalAssessmentPoll.sol";
import "./L1Block.sol";
import "./RestrictedAccess.sol";

error SettlementProposalAssessmentVerifierLottery__Unauthorized();
error SettlementProposalAssessmentVerifierLottery__SubmitterCannotParticipate(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__NotActive(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__Expired(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__NotEnoughFunds();
error SettlementProposalAssessmentVerifierLottery__AlreadyInitialized(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__InvalidClaim(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__AlreadyJoined(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__StillInProgress(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__InvalidNumberOfWinners(
    bytes32 thingProposalId
);
error SettlementProposalAssessmentVerifierLottery__InvalidReveal(
    bytes32 thingProposalId
);

contract SettlementProposalAssessmentVerifierLottery {
    struct Commitment {
        bytes32 dataHash;
        bytes32 userXorDataHash;
        int256 block;
    }

    uint256 private constant MAX_NONCE = 10_000_000;

    L1Block private constant L1BLOCK =
        L1Block(0x4200000000000000000000000000000000000015);

    TruQuest private immutable i_truQuest;
    ThingValidationPoll private s_thingValidationPoll;
    SettlementProposalAssessmentPoll private s_settlementProposalAssessmentPoll;
    address private s_orchestrator;

    uint8 public s_numVerifiers;
    uint16 public s_durationBlocks;

    bytes32[] private s_thingProposals;
    mapping(bytes32 => Commitment)
        private s_thingProposalIdToOrchestratorCommitment;
    mapping(bytes32 => mapping(address => uint256))
        private s_thingProposalIdToParticipantJoinedBlockNo;
    mapping(bytes32 => address[]) private s_thingProposalIdToParticipants;
    mapping(bytes32 => address[]) private s_thingProposalIdToClaimants;

    event LotteryInitialized(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        uint256 l1BlockNumber,
        address orchestrator,
        bytes32 dataHash,
        bytes32 userXorDataHash
    );

    event ClaimedLotterySpot(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        uint256 l1BlockNumber
    );

    event JoinedLottery(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        bytes32 userData,
        uint256 l1BlockNumber
    );

    event LotteryClosedWithSuccess(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address orchestrator,
        bytes32 data,
        bytes32 userXorData,
        bytes32 hashOfL1EndBlock,
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
            revert SettlementProposalAssessmentVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert SettlementProposalAssessmentVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier onlySettlementProposalAssessmentPoll() {
        if (msg.sender != address(s_settlementProposalAssessmentPoll)) {
            revert SettlementProposalAssessmentVerifierLottery__Unauthorized();
        }
        _;
    }

    modifier whenHasEnoughFundsToStakeAsVerifier() {
        if (!i_truQuest.checkHasEnoughFundsToStakeAsVerifier(msg.sender)) {
            revert SettlementProposalAssessmentVerifierLottery__NotEnoughFunds();
        }
        _;
    }

    modifier onlyUninitialized(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToOrchestratorCommitment[_thingProposalId].block !=
            0
        ) {
            revert SettlementProposalAssessmentVerifierLottery__AlreadyInitialized(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenNotAlreadyJoined(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToParticipantJoinedBlockNo[_thingProposalId][
                msg.sender
            ] > 0
        ) {
            revert SettlementProposalAssessmentVerifierLottery__AlreadyJoined(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenActiveAndNotExpired(bytes32 _thingProposalId) {
        int256 lotteryInitBlock = s_thingProposalIdToOrchestratorCommitment[
            _thingProposalId
        ].block;
        if (lotteryInitBlock < 1) {
            revert SettlementProposalAssessmentVerifierLottery__NotActive(
                _thingProposalId
            );
        }
        if (
            _getL1BlockNumber() > uint256(lotteryInitBlock) + s_durationBlocks
        ) {
            revert SettlementProposalAssessmentVerifierLottery__Expired(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenActive(bytes32 _thingProposalId) {
        if (
            s_thingProposalIdToOrchestratorCommitment[_thingProposalId].block <
            1
        ) {
            revert SettlementProposalAssessmentVerifierLottery__NotActive(
                _thingProposalId
            );
        }
        _;
    }

    modifier whenExpired(bytes32 _thingProposalId) {
        if (
            _getL1BlockNumber() <=
            uint256(
                s_thingProposalIdToOrchestratorCommitment[_thingProposalId]
                    .block
            ) +
                s_durationBlocks
        ) {
            revert SettlementProposalAssessmentVerifierLottery__StillInProgress(
                _thingProposalId
            );
        }
        _;
    }

    modifier onlyIfWhitelisted() {
        if (!i_truQuest.checkHasAccess(msg.sender)) {
            revert RestrictedAccess__Forbidden();
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint8 _numVerifiers,
        uint16 _durationBlocks
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = msg.sender;
        s_numVerifiers = _numVerifiers;
        s_durationBlocks = _durationBlocks;
    }

    function setPolls(
        address _thingValidationPollAddress,
        address _settlementProposalAssessmentPollAddress
    ) external onlyOrchestrator {
        s_thingValidationPoll = ThingValidationPoll(
            _thingValidationPollAddress
        );
        s_settlementProposalAssessmentPoll = SettlementProposalAssessmentPoll(
            _settlementProposalAssessmentPollAddress
        );
    }

    function exportData()
        external
        view
        returns (
            bytes32[] memory thingProposalIds,
            Commitment[] memory orchestratorCommitments,
            address[][] memory participants,
            address[][] memory claimants,
            uint256[][] memory blockNumbers
        )
    {
        thingProposalIds = s_thingProposals;
        orchestratorCommitments = new Commitment[](thingProposalIds.length);
        participants = new address[][](thingProposalIds.length);
        claimants = new address[][](thingProposalIds.length);
        blockNumbers = new uint256[][](thingProposalIds.length);
        for (uint256 i = 0; i < thingProposalIds.length; ++i) {
            bytes32 thingProposalId = thingProposalIds[i];
            orchestratorCommitments[
                i
            ] = s_thingProposalIdToOrchestratorCommitment[thingProposalId];
            participants[i] = s_thingProposalIdToParticipants[thingProposalId]; // could be empty if closed
            claimants[i] = s_thingProposalIdToClaimants[thingProposalId]; // could be empty if closed
            blockNumbers[i] = new uint256[](
                participants[i].length + claimants[i].length
            );
            uint256 j = 0;
            for (; j < participants[i].length; ++j) {
                blockNumbers[i][
                    j
                ] = s_thingProposalIdToParticipantJoinedBlockNo[
                    thingProposalId
                ][participants[i][j]];
            }
            uint256 k = 0;
            for (; j < blockNumbers[i].length; ++j) {
                blockNumbers[i][
                    j
                ] = s_thingProposalIdToParticipantJoinedBlockNo[
                    thingProposalId
                ][claimants[i][k++]];
            }
        }
    }

    function importData(
        bytes32[] calldata _thingProposalIds,
        Commitment[] calldata _orchestratorCommitments,
        address[][] calldata _participants,
        address[][] calldata _claimants,
        uint256[][] calldata _blockNumbers
    ) external onlyOrchestrator {
        s_thingProposals = _thingProposalIds;
        for (uint256 i = 0; i < _thingProposalIds.length; ++i) {
            bytes32 thingProposalId = _thingProposalIds[i];
            s_thingProposalIdToOrchestratorCommitment[
                thingProposalId
            ] = _orchestratorCommitments[i];
            s_thingProposalIdToParticipants[thingProposalId] = _participants[i];
            s_thingProposalIdToClaimants[thingProposalId] = _claimants[i];
            uint256 j = 0;
            for (; j < _participants[i].length; ++j) {
                s_thingProposalIdToParticipantJoinedBlockNo[thingProposalId][
                    _participants[i][j]
                ] = _blockNumbers[i][j];
            }
            uint256 k = 0;
            for (; j < _blockNumbers[i].length; ++j) {
                s_thingProposalIdToParticipantJoinedBlockNo[thingProposalId][
                    _claimants[i][k++]
                ] = _blockNumbers[i][j];
            }
        }
    }

    function _getL1BlockNumber() private view returns (uint256) {
        if (block.chainid == 901) {
            return L1BLOCK.number();
        }
        return block.number;
    }

    function computeHash(bytes32 _data) public view returns (bytes32) {
        return keccak256(abi.encodePacked(address(this), _data));
    }

    function getLotteryInitBlock(
        bytes32 _thingProposalId
    ) external view returns (int256) {
        return
            s_thingProposalIdToOrchestratorCommitment[_thingProposalId].block;
    }

    function checkAlreadyClaimedLotterySpot(
        bytes32 _thingProposalId,
        address _user
    ) external view returns (bool) {
        if (
            s_thingProposalIdToParticipantJoinedBlockNo[_thingProposalId][
                _user
            ] == 0
        ) {
            return false;
        }

        address[] memory claimants = s_thingProposalIdToClaimants[
            _thingProposalId
        ];
        for (uint256 i = 0; i < claimants.length; ++i) {
            if (claimants[i] == _user) {
                return true;
            }
        }

        return false;
    }

    function checkAlreadyJoinedLottery(
        bytes32 _thingProposalId,
        address _user
    ) external view returns (bool) {
        if (
            s_thingProposalIdToParticipantJoinedBlockNo[_thingProposalId][
                _user
            ] == 0
        ) {
            return false;
        }

        address[] memory participants = s_thingProposalIdToParticipants[
            _thingProposalId
        ];
        for (uint256 i = 0; i < participants.length; ++i) {
            if (participants[i] == _user) {
                return true;
            }
        }

        return false;
    }

    function getSpotClaimants(
        bytes32 _thingProposalId
    ) external view returns (address[] memory) {
        return s_thingProposalIdToClaimants[_thingProposalId];
    }

    function getParticipants(
        bytes32 _thingProposalId
    ) external view returns (address[] memory) {
        return s_thingProposalIdToParticipants[_thingProposalId];
    }

    function _splitIds(
        bytes32 _thingProposalId
    ) private pure returns (bytes16 _thingId, bytes16 _settlementProposalId) {
        _thingId = bytes16(_thingProposalId);
        _settlementProposalId = bytes16(uint128(uint256(_thingProposalId)));
    }

    function initLottery(
        bytes32 _thingProposalId,
        bytes32 _dataHash,
        bytes32 _userXorDataHash
    ) external onlyOrchestrator onlyUninitialized(_thingProposalId) {
        uint256 l1BlockNumber = _getL1BlockNumber();
        s_thingProposals.push(_thingProposalId);
        s_thingProposalIdToOrchestratorCommitment[
            _thingProposalId
        ] = Commitment(_dataHash, _userXorDataHash, int256(l1BlockNumber));

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        // @@TODO: Finish this.
        // bytes16 currentProposalId = i_truQuest.getSettlementProposalId(
        //     thingId
        // );

        // if (proposalId != currentProposalId) {
        //     // ...
        // }

        emit LotteryInitialized(
            thingId,
            proposalId,
            l1BlockNumber,
            s_orchestrator,
            _dataHash,
            _userXorDataHash
        );
    }

    function claimLotterySpot(
        bytes32 _thingProposalId,
        uint16 _thingVerifiersArrayIndex
    )
        external
        whenHasEnoughFundsToStakeAsVerifier
        whenActiveAndNotExpired(_thingProposalId)
        whenNotAlreadyJoined(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        if (i_truQuest.getSettlementProposalSubmitter(thingId) == msg.sender) {
            revert SettlementProposalAssessmentVerifierLottery__SubmitterCannotParticipate(
                _thingProposalId
            );
        }

        if (
            !s_thingValidationPoll.checkUserIsThingsVerifierAtIndex(
                thingId,
                msg.sender,
                _thingVerifiersArrayIndex
            )
        ) {
            revert SettlementProposalAssessmentVerifierLottery__InvalidClaim(
                _thingProposalId
            );
        }

        i_truQuest.stakeAsVerifier(msg.sender);
        uint256 l1BlockNumber = _getL1BlockNumber();
        s_thingProposalIdToParticipantJoinedBlockNo[_thingProposalId][
            msg.sender
        ] = l1BlockNumber;
        s_thingProposalIdToClaimants[_thingProposalId].push(msg.sender);

        emit ClaimedLotterySpot(thingId, proposalId, msg.sender, l1BlockNumber);
    }

    function joinLottery(
        bytes32 _thingProposalId,
        bytes32 _userData
    )
        external
        onlyIfWhitelisted
        whenHasEnoughFundsToStakeAsVerifier
        whenActiveAndNotExpired(_thingProposalId)
        whenNotAlreadyJoined(_thingProposalId)
    {
        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        if (i_truQuest.getSettlementProposalSubmitter(thingId) == msg.sender) {
            revert SettlementProposalAssessmentVerifierLottery__SubmitterCannotParticipate(
                _thingProposalId
            );
        }

        i_truQuest.stakeAsVerifier(msg.sender);
        uint256 l1BlockNumber = _getL1BlockNumber();
        s_thingProposalIdToParticipantJoinedBlockNo[_thingProposalId][
            msg.sender
        ] = l1BlockNumber;
        s_thingProposalIdToParticipants[_thingProposalId].push(msg.sender);

        emit JoinedLottery(
            thingId,
            proposalId,
            msg.sender,
            _userData,
            l1BlockNumber
        );
    }

    function checkExpired(
        bytes32 _thingProposalId
    ) external view returns (bool) {
        return
            _getL1BlockNumber() >
            uint256(
                s_thingProposalIdToOrchestratorCommitment[_thingProposalId]
                    .block
            ) +
                s_durationBlocks;
    }

    function getMaxNonce() external pure returns (uint256) {
        return MAX_NONCE;
    }

    function _getWinnerClaimants(
        bytes32 _thingProposalId,
        uint64[] calldata _winnerClaimantIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory claimants = s_thingProposalIdToClaimants[
            _thingProposalId
        ];
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

        delete s_thingProposalIdToClaimants[_thingProposalId];

        return winners;
    }

    function _getLotteryWinners(
        bytes32 _thingProposalId,
        uint64[] calldata _winnerIndices
    ) private returns (address[] memory) {
        uint64 j = 0;
        address[] memory participants = s_thingProposalIdToParticipants[
            _thingProposalId
        ];
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

        delete s_thingProposalIdToParticipants[_thingProposalId];

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
        bytes32 _userXorData,
        bytes32 _hashOfL1EndBlock,
        uint64[] calldata _winnerClaimantIndices,
        uint64[] calldata _winnerIndices
    )
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        if (
            _winnerClaimantIndices.length + _winnerIndices.length !=
            s_numVerifiers
        ) {
            revert SettlementProposalAssessmentVerifierLottery__InvalidNumberOfWinners(
                _thingProposalId
            );
        }

        Commitment
            memory commitment = s_thingProposalIdToOrchestratorCommitment[
                _thingProposalId
            ];

        if (computeHash(_data) != commitment.dataHash) {
            revert SettlementProposalAssessmentVerifierLottery__InvalidReveal(
                _thingProposalId
            );
        }
        if (computeHash(_userXorData) != commitment.userXorDataHash) {
            revert SettlementProposalAssessmentVerifierLottery__InvalidReveal(
                _thingProposalId
            );
        }

        s_thingProposalIdToOrchestratorCommitment[_thingProposalId]
            .block = -commitment.block;

        // @@NOTE: Have to split the function into 2 to avoid "Stack too deep" error.
        _finishClosingLotteryWithSuccess(
            _thingProposalId,
            _data,
            _userXorData,
            _hashOfL1EndBlock,
            _winnerClaimantIndices,
            _winnerIndices
        );
    }

    function _finishClosingLotteryWithSuccess(
        bytes32 _thingProposalId,
        bytes32 _data,
        bytes32 _userXorData,
        bytes32 _hashOfL1EndBlock,
        uint64[] calldata _winnerClaimantIndices,
        uint64[] calldata _winnerIndices
    ) private {
        address[] memory claimants = _getWinnerClaimants(
            _thingProposalId,
            _winnerClaimantIndices
        );

        address[] memory winners = _getLotteryWinners(
            _thingProposalId,
            _winnerIndices
        );

        s_settlementProposalAssessmentPoll.initPoll(
            _thingProposalId,
            _concatWinners(claimants, winners)
        );

        uint256 nonce = (uint256(_data) ^ uint256(_hashOfL1EndBlock)) %
            MAX_NONCE;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        s_thingValidationPoll.clearThingVerifiers(thingId);

        emit LotteryClosedWithSuccess(
            thingId,
            proposalId,
            s_orchestrator,
            _data,
            _userXorData,
            _hashOfL1EndBlock,
            nonce,
            claimants,
            winners
        );
    }

    function closeLotteryInFailure(
        bytes32 _thingProposalId,
        uint8 _joinedNumVerifiers
    )
        external
        onlyOrchestrator
        whenActive(_thingProposalId)
        whenExpired(_thingProposalId)
    {
        s_thingProposalIdToOrchestratorCommitment[_thingProposalId]
            .block = -s_thingProposalIdToOrchestratorCommitment[
            _thingProposalId
        ].block;

        (bytes16 thingId, bytes16 proposalId) = _splitIds(_thingProposalId);

        address submitter = i_truQuest.getSettlementProposalSubmitter(thingId);
        i_truQuest.unstakeSettlementProposalSubmitter(submitter);

        address[] memory claimants = s_thingProposalIdToClaimants[
            _thingProposalId
        ];
        for (uint64 i = 0; i < claimants.length; ++i) {
            i_truQuest.unstakeAsVerifier(claimants[i]);
        }

        address[] memory participants = s_thingProposalIdToParticipants[
            _thingProposalId
        ];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstakeAsVerifier(participants[i]);
        }

        delete s_thingProposalIdToClaimants[_thingProposalId];
        delete s_thingProposalIdToParticipants[_thingProposalId];

        i_truQuest.setThingNoLongerHasSettlementProposalUnderAssessment(
            thingId
        );

        s_thingValidationPoll.clearThingVerifiers(thingId);

        emit LotteryClosedInFailure(
            thingId,
            proposalId,
            s_numVerifiers,
            _joinedNumVerifiers
        );
    }
}
