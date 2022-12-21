namespace Application.Common.Interfaces;

public interface IContractStorageQueryable
{
    Task<int> GetThingSubmissionVerifierLotteryDurationBlocks();
    Task<int> GetAcceptancePollDurationBlocks();
    Task<int> GetThingSubmissionNumVerifiers();
    Task<string> GetThingSubmissionVerifierLotteryParticipantAt(byte[] thingId, int index);
    Task<int> GetThingAssessmentVerifierLotteryDurationBlocks();
    Task<int> GetThingAssessmentNumVerifiers();
    Task<string> GetThingAssessmentVerifierLotterySpotClaimantAt(byte[] thingId, int index);
    Task<string> GetThingAssessmentVerifierLotteryParticipantAt(byte[] thingId, int index);
}