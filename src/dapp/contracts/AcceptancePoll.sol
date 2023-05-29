// SPDX-License-Identifier: MIT
pragma solidity >=0.8.0 <0.9.0;

import "./TruQuest.sol";
import "./ThingSubmissionVerifierLottery.sol";

error AcceptancePoll__Unauthorized();
error AcceptancePoll__Expired(bytes16 thingId);
error AcceptancePoll__NotDesignatedVerifier(bytes16 thingId);
error AcceptancePoll__NotInProgress(bytes16 thingId);

contract AcceptancePoll {
    enum Vote {
        Decline__Soft,
        Decline__Hard,
        Accept
    }

    enum Decision {
        Unsettled__InsufficientVotingVolume,
        Unsettled__MajorityThresholdNotReached,
        Declined__Soft,
        Declined__Hard,
        Accepted
    }

    enum Stage {
        None,
        InProgress,
        Finalized
    }

    TruQuest private immutable i_truQuest;
    ThingSubmissionVerifierLottery private s_verifierLottery;
    address private s_orchestrator;

    uint16 private s_durationBlocks;
    uint8 private s_votingVolumeThresholdPercent;
    uint8 private s_majorityThresholdPercent;

    mapping(bytes16 => uint64) private s_thingIdToPollInitBlock;
    mapping(bytes16 => address[]) private s_thingVerifiers;
    mapping(bytes16 => Stage) private s_thingPollStage;

    event CastedVote(bytes16 indexed thingId, address indexed user, Vote vote);

    event CastedVoteWithReason(
        bytes16 indexed thingId,
        address indexed user,
        Vote vote,
        string reason
    );

    event PollFinalized(
        bytes16 indexed thingId,
        Decision decision,
        string voteAggIpfsCid,
        address submitter,
        address[] rewardedVerifiers,
        address[] slashedVerifiers
    );

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert AcceptancePoll__Unauthorized();
        }
        _;
    }

    modifier onlyTruQuest() {
        if (msg.sender != address(i_truQuest)) {
            revert AcceptancePoll__Unauthorized();
        }
        _;
    }

    modifier onlyThingSubmissionVerifierLottery() {
        if (msg.sender != address(s_verifierLottery)) {
            revert AcceptancePoll__Unauthorized();
        }
        _;
    }

    modifier onlyWhileNotExpired(bytes16 _thingId) {
        if (
            block.number > s_thingIdToPollInitBlock[_thingId] + s_durationBlocks
        ) {
            revert AcceptancePoll__Expired(_thingId);
        }
        _;
    }

    modifier onlyDesignatedVerifiers(bytes16 _thingId) {
        uint256 designatedVerifiersCount = s_thingVerifiers[_thingId].length; // @@??: static var ?
        bool isDesignatedVerifier = false;
        // while?
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (msg.sender == s_thingVerifiers[_thingId][i]) {
                // @@??: No point saving array in memory ?
                isDesignatedVerifier = true;
                break;
            }
        }
        if (!isDesignatedVerifier) {
            revert AcceptancePoll__NotDesignatedVerifier(_thingId);
        }
        _;
    }

    modifier onlyWhenInProgress(bytes16 _thingId) {
        if (s_thingPollStage[_thingId] != Stage.InProgress) {
            revert AcceptancePoll__NotInProgress(_thingId);
        }
        _;
    }

    constructor(
        address _truQuestAddress,
        uint16 _durationBlocks,
        uint8 _votingVolumeThresholdPercent,
        uint8 _majorityThresholdPercent
    ) {
        i_truQuest = TruQuest(_truQuestAddress);
        s_orchestrator = tx.origin;
        s_durationBlocks = _durationBlocks;
        s_votingVolumeThresholdPercent = _votingVolumeThresholdPercent;
        s_majorityThresholdPercent = _majorityThresholdPercent;
    }

    function connectToThingSubmissionVerifierLottery(
        address _verifierLotteryAddress
    ) external onlyTruQuest {
        s_verifierLottery = ThingSubmissionVerifierLottery(
            _verifierLotteryAddress
        );
    }

    function getPollDurationBlocks() public view returns (uint16) {
        return s_durationBlocks;
    }

    function getPollInitBlock(bytes16 _thingId) public view returns (uint64) {
        return s_thingIdToPollInitBlock[_thingId];
    }

    function initPoll(
        bytes16 _thingId,
        address[] memory _verifiers
    ) external onlyThingSubmissionVerifierLottery {
        s_thingIdToPollInitBlock[_thingId] = uint64(block.number);
        s_thingVerifiers[_thingId] = _verifiers;
        s_thingPollStage[_thingId] = Stage.InProgress;
    }

    function checkIsDesignatedVerifierForThing(
        bytes16 _thingId,
        address _user
    ) public view returns (bool) {
        uint256 designatedVerifiersCount = s_thingVerifiers[_thingId].length;
        for (uint8 i = 0; i < designatedVerifiersCount; ++i) {
            if (_user == s_thingVerifiers[_thingId][i]) {
                return true;
            }
        }

        return false;
    }

    function castVote(
        bytes16 _thingId,
        Vote _vote
    )
        public
        onlyWhenInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedVote(_thingId, msg.sender, _vote);
    }

    function castVoteWithReason(
        bytes16 _thingId,
        Vote _vote,
        string calldata _reason
    )
        public
        onlyWhenInProgress(_thingId)
        onlyWhileNotExpired(_thingId)
        onlyDesignatedVerifiers(_thingId)
    {
        emit CastedVoteWithReason(_thingId, msg.sender, _vote, _reason);
    }

    function getVerifiers(
        bytes16 _thingId
    ) public view returns (address[] memory) {
        return s_thingVerifiers[_thingId];
    }

    function finalizePoll__Unsettled(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        Decision _decision,
        uint64[] calldata _verifiersToSlashIndices
    ) public onlyOrchestrator onlyWhenInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeThingSubmitter(submitter);

        uint64 j = 0;
        address[] memory verifiers = s_thingVerifiers[_thingId];
        address[] memory slashedVerifiers = new address[](
            _verifiersToSlashIndices.length
        );
        for (uint8 i = 0; i < _verifiersToSlashIndices.length; ++i) {
            uint64 nextVerifierToSlashIndex = _verifiersToSlashIndices[i];
            slashedVerifiers[i] = verifiers[nextVerifierToSlashIndex];
            for (; j < nextVerifierToSlashIndex; ++j) {
                i_truQuest.unstakeAsVerifier(verifiers[j]);
            }
            i_truQuest.unstakeAndSlashVerifier(verifiers[j++]);
        }

        emit PollFinalized(
            _thingId,
            _decision,
            _voteAggIpfsCid,
            submitter,
            new address[](0),
            slashedVerifiers
        );
    }

    function finalizePoll__Accepted(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeAndRewardThingSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            _thingId,
            Decision.Accepted,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Soft(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeThingSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            _thingId,
            Decision.Declined__Soft,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }

    function finalizePoll__Declined__Hard(
        bytes16 _thingId,
        string calldata _voteAggIpfsCid,
        address[] calldata _verifiersToReward,
        address[] calldata _verifiersToSlash
    ) public onlyOrchestrator onlyWhenInProgress(_thingId) {
        s_thingPollStage[_thingId] = Stage.Finalized;
        address submitter = i_truQuest.s_thingSubmitter(_thingId);
        i_truQuest.unstakeAndSlashThingSubmitter(submitter);

        for (uint8 i = 0; i < _verifiersToReward.length; ++i) {
            i_truQuest.unstakeAndRewardVerifier(_verifiersToReward[i]);
        }

        for (uint8 i = 0; i < _verifiersToSlash.length; ++i) {
            i_truQuest.unstakeAndSlashVerifier(_verifiersToSlash[i]);
        }

        emit PollFinalized(
            _thingId,
            Decision.Declined__Hard,
            _voteAggIpfsCid,
            submitter,
            _verifiersToReward,
            _verifiersToSlash
        );
    }
}
