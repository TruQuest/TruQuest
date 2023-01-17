namespace Application.Thing.Queries.GetThing;

public class GetThingResultVm
{
    public required ThingQm Thing { get; init; }
    public required string? Signature { get; init; }
}