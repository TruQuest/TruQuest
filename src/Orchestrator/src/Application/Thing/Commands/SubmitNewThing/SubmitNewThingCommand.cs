using Microsoft.Extensions.Logging;

using GoThataway;
using FluentValidation;

using Domain.Results;
using Domain.Aggregates;
using Domain.Errors;

using Application.Common.Attributes;
using Application.Common.Interfaces;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Thing.Commands.SubmitNewThing;

[RequireAuthorization, ExecuteInTxn]
public class SubmitNewThingCommand : IRequest<HandleResult<SubmitNewThingResultVm>>
{
    public required Guid ThingId { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(HandleResult<SubmitNewThingResultVm> _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId)
        };
    }
}

internal class Validator : AbstractValidator<SubmitNewThingCommand>
{
    public Validator()
    {
        RuleFor(c => c.ThingId).NotEmpty();
    }
}

public class SubmitNewThingCommandHandler : IRequestHandler<SubmitNewThingCommand, HandleResult<SubmitNewThingResultVm>>
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
        if (thing.SubmitterId != _currentPrincipal.Id!)
        {
            _logger.LogWarning($"User {UserId} trying to submit a thing {ThingId} created by another", _currentPrincipal.Id!, thing.Id);
            return new()
            {
                Error = new HandleError("Invalid request")
            };
        }
        if (thing.State != ThingState.Draft)
        {
            _logger.LogWarning($"User {UserId} trying to submit an already submitted thing {ThingId}", _currentPrincipal.Id!, thing.Id);
            return new()
            {
                Error = new HandleError("Already submitted")
            };
        }

        thing.SetState(ThingState.AwaitingFunding);

        await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
            thingId: thing.Id,
            category: ThingUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Promise submitted",
            details: "Click to refresh the page"
        ));

        await _thingRepository.SaveChanges();
        await _thingUpdateRepository.SaveChanges();

        _logger.LogInformation($"Thing {ThingId} successfully submitted", thing.Id);

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
