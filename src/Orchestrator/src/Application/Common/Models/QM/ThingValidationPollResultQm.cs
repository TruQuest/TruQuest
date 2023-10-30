namespace Application.Common.Models.QM;

public class ThingValidationPollResultQm
{
    public required ThingStateQm State { get; init; }
    public required string? VoteAggIpfsCid { get; init; }
}
