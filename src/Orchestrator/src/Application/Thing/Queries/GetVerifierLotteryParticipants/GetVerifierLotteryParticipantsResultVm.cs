namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsResultVm
{
    public required Guid ThingId { get; init; }
    public required IEnumerable<VerifierLotteryParticipantEntryQm> Entries { get; init; }
}