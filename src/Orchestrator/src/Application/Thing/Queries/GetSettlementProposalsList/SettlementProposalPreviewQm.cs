using System.Text.Json.Serialization;

using Domain.Aggregates;

namespace Application.Thing.Queries.GetSettlementProposalsList;

public class SettlementProposalPreviewQm
{
    public required Guid Id { get; init; }
    public required SettlementProposalState State { get; init; }
    // @@NOTE: Can't ignore because for some reason ignoring it
    // breaks SettlementProposalQueryable.GetForThing EF query. Why though?
    // "Marked as required but there is no setter" 
    // [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public required long? SubmittedAt { get; init; }
    public required string Title { get; init; }
    public required Verdict Verdict { get; init; }
    public required string? CroppedImageIpfsCid { get; init; }
    public required string SubmitterId { get; init; }
    // [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public required long? AssessmentPronouncedAt { get; init; }
    public long? DisplayedTimestamp { get; set; }
}