// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./AcceptancePoll.sol";
import "./L1Block.sol";

error ThingSubmissionVerifierLottery__Unauthorized();
error ThingSubmissionVerifierLottery__SubmitterCannotParticipate(
    bytes16 thingId
);
error ThingSubmissionVerifierLottery__NotActive(bytes16 thingId);
error ThingSubmissionVerifierLottery__Expired(bytes16 thingId);
error ThingSubmissionVerifierLottery__NotEnoughFunds();
error ThingSubmissionVerifierLottery__AlreadyInitialized(bytes16 thingId);
error ThingSubmissionVerifierLottery__AlreadyJoined(bytes16 thingId);
error ThingSubmissionVerifierLottery__StillInProgress(bytes16 thingId);
error ThingSubmissionVerifierLottery__InvalidNumberOfWinners(bytes16 thingId);
error ThingSubmissionVerifierLottery__InvalidReveal(bytes16 thingId);

contract ThingSubmissionVerifierLottery {
    struct Commitment {
        bytes32 dataHash;
        bytes32 userXorDataHash;
        int256 block;
    }

    uint256 private constant MAX_NONCE = 10_000_000;

    L1Block private constant L1BLOCK =
        L1Block(0x4200000000000000000000000000000000000015);

    TruQuest private immutable i_truQuest;
    AcceptancePoll private s_acceptancePoll;
    address private s_orchestrator;

    uint8 public s_numVerifiers;
    uint16 public s_durationBlocks;

    mapping(bytes16 => Commitment) private s_thingIdToOrchestratorCommitment;
    mapping(bytes16 => mapping(address => uint256))
        private s_thingIdToParticipantJoinedBlockNo;
    mapping(bytes16 => address[]) private s_thingIdToParticipants;

    event LotteryInitialized(
        bytes16 indexed thingId,
        uint256 l1BlockNumber,
        address orchestrator,
        bytes32 dataHash,
        bytes32 userXorDataHash
    );

    event JoinedLottery(
        bytes16 indexed thingId,
        address indexed user,
        bytes32 userData,
        uint256 l1BlockNumber
    );

    event LotteryClosedWithSuccess(
        bytes16 indexed thingId,
        address orchestrator,
        bytes32 data,
        bytes32 userXorData,
        bytes32 hashOfL1EndBlock,
        uint256 nonce,
        address[] winners
    );

    event LotteryClosedInFailure(
        bytes16 indexed thingId,
        uint8 requiredNumVerifiers,
        uint8 joinedNumVerifiers
    );

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

    modifier notSubmitter(bytes16 _thingId) {
        if (i_truQuest.s_thingSubmitter(_thingId) == msg.sender) {
            revert ThingSubmissionVerifierLottery__SubmitterCannotParticipate(
                _thingId
            );
        }
        _;
    }

    modifier whenHasEnoughFundsToStakeAsVerifier() {
        if (!i_truQuest.checkHasEnoughFundsToStakeAsVerifier(msg.sender)) {
            revert ThingSubmissionVerifierLottery__NotEnoughFunds();
        }
        _;
    }

    modifier onlyUninitialized(bytes16 _thingId) {
        if (s_thingIdToOrchestratorCommitment[_thingId].block != 0) {
            revert ThingSubmissionVerifierLottery__AlreadyInitialized(_thingId);
        }
        _;
    }

    modifier whenNotAlreadyJoined(bytes16 _thingId) {
        if (s_thingIdToParticipantJoinedBlockNo[_thingId][msg.sender] > 0) {
            revert ThingSubmissionVerifierLottery__AlreadyJoined(_thingId);
        }
        _;
    }

    modifier whenActiveAndNotExpired(bytes16 _thingId) {
        int256 lotteryInitBlock = s_thingIdToOrchestratorCommitment[_thingId]
            .block;
        if (lotteryInitBlock < 1) {
            revert ThingSubmissionVerifierLottery__NotActive(_thingId);
        }
        if (
            _getL1BlockNumber() > uint256(lotteryInitBlock) + s_durationBlocks
        ) {
            revert ThingSubmissionVerifierLottery__Expired(_thingId);
        }
        _;
    }

    modifier whenActive(bytes16 _thingId) {
        if (s_thingIdToOrchestratorCommitment[_thingId].block < 1) {
            revert ThingSubmissionVerifierLottery__NotActive(_thingId);
        }
        _;
    }

    modifier whenExpired(bytes16 _thingId) {
        if (
            _getL1BlockNumber() <=
            uint256(s_thingIdToOrchestratorCommitment[_thingId].block) +
                s_durationBlocks
        ) {
            revert ThingSubmissionVerifierLottery__StillInProgress(_thingId);
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

    function setAcceptancePoll(
        address _acceptancePollAddress
    ) external onlyOrchestrator {
        s_acceptancePoll = AcceptancePoll(_acceptancePollAddress);
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
        bytes16 _thingId
    ) external view returns (int256) {
        return s_thingIdToOrchestratorCommitment[_thingId].block;
    }

    function checkAlreadyJoinedLottery(
        bytes16 _thingId,
        address _user
    ) external view returns (bool) {
        return s_thingIdToParticipantJoinedBlockNo[_thingId][_user] > 0;
    }

    function getParticipants(
        bytes16 _thingId
    ) external view returns (address[] memory) {
        return s_thingIdToParticipants[_thingId];
    }

    // @@TODO: Check that the thing is funded.
    function initLottery(
        bytes16 _thingId,
        bytes32 _dataHash,
        bytes32 _userXorDataHash
    ) external onlyOrchestrator onlyUninitialized(_thingId) {
        uint256 l1BlockNumber = _getL1BlockNumber();
        s_thingIdToOrchestratorCommitment[_thingId] = Commitment(
            _dataHash,
            _userXorDataHash,
            int256(l1BlockNumber)
        );

        emit LotteryInitialized(
            _thingId,
            l1BlockNumber,
            s_orchestrator,
            _dataHash,
            _userXorDataHash
        );
    }

    function joinLottery(
        bytes16 _thingId,
        bytes32 _userData
    )
        external
        notSubmitter(_thingId)
        whenHasEnoughFundsToStakeAsVerifier
        whenActiveAndNotExpired(_thingId)
        whenNotAlreadyJoined(_thingId)
    {
        // @@??: Should check that _userData != bytes32(0)?
        i_truQuest.stakeAsVerifier(msg.sender);
        uint256 l1BlockNumber = _getL1BlockNumber();
        s_thingIdToParticipantJoinedBlockNo[_thingId][
            msg.sender
        ] = l1BlockNumber;
        s_thingIdToParticipants[_thingId].push(msg.sender);

        emit JoinedLottery(_thingId, msg.sender, _userData, l1BlockNumber);
    }

    function checkExpired(bytes16 _thingId) external view returns (bool) {
        return
            _getL1BlockNumber() >
            uint256(s_thingIdToOrchestratorCommitment[_thingId].block) +
                s_durationBlocks;
    }

    function getMaxNonce() external pure returns (uint256) {
        return MAX_NONCE;
    }

    function closeLotteryWithSuccess(
        bytes16 _thingId,
        bytes32 _data,
        bytes32 _userXorData,
        bytes32 _hashOfL1EndBlock,
        uint64[] calldata _winnerIndices // sorted asc indices of users in participants array
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        if (_winnerIndices.length != s_numVerifiers) {
            revert ThingSubmissionVerifierLottery__InvalidNumberOfWinners(
                _thingId
            );
        }

        Commitment memory commitment = s_thingIdToOrchestratorCommitment[
            _thingId
        ];

        if (computeHash(_data) != commitment.dataHash) {
            revert ThingSubmissionVerifierLottery__InvalidReveal(_thingId);
        }
        if (computeHash(_userXorData) != commitment.userXorDataHash) {
            revert ThingSubmissionVerifierLottery__InvalidReveal(_thingId);
        }

        s_thingIdToOrchestratorCommitment[_thingId].block = -commitment.block;

        uint64 j = 0;
        address[] memory participants = s_thingIdToParticipants[_thingId];
        address[] memory winners = new address[](_winnerIndices.length);
        for (uint8 i = 0; i < _winnerIndices.length; ++i) {
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

        s_thingIdToParticipants[_thingId] = new address[](0); // @@??: Unnecessary?
        delete s_thingIdToParticipants[_thingId];

        s_acceptancePoll.initPoll(_thingId, winners);

        uint256 nonce = (uint256(_data) ^ uint256(_hashOfL1EndBlock)) %
            MAX_NONCE;

        emit LotteryClosedWithSuccess(
            _thingId,
            s_orchestrator,
            _data,
            _userXorData,
            _hashOfL1EndBlock,
            nonce,
            winners
        );
    }

    function closeLotteryInFailure(
        bytes16 _thingId,
        uint8 _joinedNumVerifiers
    ) external onlyOrchestrator whenActive(_thingId) whenExpired(_thingId) {
        // @@??: Pass and check data?
        s_thingIdToOrchestratorCommitment[_thingId]
            .block = -s_thingIdToOrchestratorCommitment[_thingId].block;

        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeThingSubmitter(submitter);

        address[] memory participants = s_thingIdToParticipants[_thingId];
        for (uint64 i = 0; i < participants.length; ++i) {
            i_truQuest.unstakeAsVerifier(participants[i]);
        }

        s_thingIdToParticipants[_thingId] = new address[](0); // @@??: Unnecessary?
        delete s_thingIdToParticipants[_thingId];

        emit LotteryClosedInFailure(
            _thingId,
            s_numVerifiers,
            _joinedNumVerifiers
        );
    }
}
