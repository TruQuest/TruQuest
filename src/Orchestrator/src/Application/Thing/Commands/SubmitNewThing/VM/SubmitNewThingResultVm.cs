namespace Application.Thing.Commands.SubmitNewThing;

public class SubmitNewThingResultVm
{
    public required ThingVm Thing { get; init; }
    public required string Signature { get; init; }
}