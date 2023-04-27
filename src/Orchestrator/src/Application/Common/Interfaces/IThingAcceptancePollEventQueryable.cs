using Domain.Aggregates.Events;

using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollEventQueryable
{
    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindJoinedEventsWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    );

    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> GetJoinedEventsFor(
        Guid thingId, IEnumerable<string> userIds
    );

    Task<IEnumerable<VerifierLotteryWinnerQm>> GetLotteryWinnerIndicesAccordingToPreJoinedEvents(
        Guid thingId, IEnumerable<string> winnerIds
    );
}