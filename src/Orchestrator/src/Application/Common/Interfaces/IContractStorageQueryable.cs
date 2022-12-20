namespace Application.Common.Interfaces;

public interface IContractStorageQueryable
{
    Task<int> GetThingSubmissionVerifierLotteryDurationBlocks();
    Task<int> GetAcceptancePollDurationBlocks();
    Task<int> GetNumVerifiers();
    Task<string> GetVerifierLotteryParticipantAt(string thingId, int index);
    Task<int> GetThingAssessmentVerifierLotteryDurationBlocks();
    Task<int> GetThingAssessmentNumVerifiers();
    Task<string> GetThingAssessmentVerifierLotterySpotClaimantAt(string thingId, int index);
    Task<string> GetThingAssessmentVerifierLotteryParticipantAt(string thingId, int index);
}