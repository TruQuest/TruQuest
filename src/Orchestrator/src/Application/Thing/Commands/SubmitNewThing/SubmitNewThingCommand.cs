using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.SubmitNewThing;

[RequireAuthorization, ExecuteInTxn]
public class SubmitNewThingCommand : IRequest<HandleResult<SubmitNewThingResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class SubmitNewThingCommandHandler : IRequestHandler<SubmitNewThingCommand, HandleResult<SubmitNewThingResultVm>>
{
    private readonly ILogger<SubmitNewThingCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IThingRepository _thingRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;

    public SubmitNewThingCommandHandler(
        ILogger<SubmitNewThingCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IThingRepository thingRepository,
        IThingUpdateRepository thingUpdateRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _thingRepository = thingRepository;
        _thingUpdateRepository = thingUpdateRepository;
    }

    public async Task<HandleResult<SubmitNewThingResultVm>> Handle(
        SubmitNewThingCommand command, CancellationToken ct
    )
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        // @@??: Should check using resource-based authorization?
        if (thing.SubmitterId != _currentPrincipal.Id!)
        {
            return new()
            {
                Error = new ThingError("Invalid request")
            };
        }
        if (thing.State != ThingState.Draft)
        {
            return new()
            {
                Error = new ThingError("Already submitted")
            };
        }

        thing.SetState(ThingState.AwaitingFunding);

        await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
            thingId: thing.Id,
            category: ThingUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Thing submitted",
            details: "Click to refresh the page"
        ));

        await _thingRepository.SaveChanges();
        await _thingUpdateRepository.SaveChanges();

        return new()
        {
            Data = new()
            {
                ThingId = thing.Id,
                Signature = _signer.SignThing(thing.Id)
            }
        };
    }
}