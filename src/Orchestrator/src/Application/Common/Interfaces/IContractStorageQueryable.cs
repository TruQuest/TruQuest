namespace Application.Common.Interfaces;

public interface IContractStorageQueryable
{
    Task<int> GetThingSubmissionVerifierLotteryDurationBlocks();
    Task<int> GetAcceptancePollDurationBlocks();
    Task<int> GetThingSubmissionNumVerifiers();
    Task<string> GetThingSubmissionVerifierLotteryParticipantAt(byte[] thingId, int index);
    Task<int> GetThingAcceptancePollVotingVolumeThreshold();
    Task<int> GetThingAcceptancePollMajorityThreshold();
    Task<int> GetThingAssessmentVerifierLotteryDurationBlocks();
    Task<int> GetThingAssessmentNumVerifiers();
    Task<int> GetThingAssessmentPollVotingVolumeThreshold();
    Task<int> GetThingAssessmentPollMajorityThreshold();
    Task<string> GetThingAssessmentVerifierLotterySpotClaimantAt(byte[] thingId, byte[] proposalId, int index);
    Task<string> GetThingAssessmentVerifierLotteryParticipantAt(byte[] thingId, byte[] proposalId, int index);
    Task<int> GetAssessmentPollDurationBlocks();
}