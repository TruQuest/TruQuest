using Domain.Aggregates.Events;

using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollEventQueryable
{
    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindJoinedEventsWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    );

    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindJoinedEventsWithClosestNoncesAmongUsers(
        Guid thingId, IEnumerable<string> userIds, decimal nonce, int count
    );

    Task<IEnumerable<VerifierLotteryWinnerQm>> GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
        Guid thingId, IEnumerable<string> winnerIds
    );
}