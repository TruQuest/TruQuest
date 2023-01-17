namespace Application.Thing.Commands.SubmitNewThing;

public class SubmitNewThingResultVm
{
    public required Guid ThingId { get; init; }
    public required string Signature { get; init; }
}