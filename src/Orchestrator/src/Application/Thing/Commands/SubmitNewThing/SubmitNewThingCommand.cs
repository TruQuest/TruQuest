using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;
using ThingDm = Domain.Aggregates.Thing;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Thing.Commands.SubmitNewThing;

[RequireAuthorization]
public class SubmitNewThingCommand : IRequest<HandleResult<SubmitNewThingResultVm>>
{
    public NewThingIm Input { get; set; }
    public string Signature { get; set; }
}

internal class SubmitNewThingCommandHandler : IRequestHandler<SubmitNewThingCommand, HandleResult<SubmitNewThingResultVm>>
{
    private readonly ILogger<SubmitNewThingCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileArchiver _fileArchiver;
    private readonly IThingRepository _thingRepository;

    public SubmitNewThingCommandHandler(
        ILogger<SubmitNewThingCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileArchiver fileArchiver,
        IThingRepository thingRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileArchiver = fileArchiver;
        _thingRepository = thingRepository;
    }

    public async Task<HandleResult<SubmitNewThingResultVm>> Handle(
        SubmitNewThingCommand command, CancellationToken ct
    )
    {
        var result = _signer.RecoverFromNewThingMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id

        await _fileArchiver.ArchiveAll(command.Input);

        var thing = new ThingDm(
            title: command.Input.Title,
            details: command.Input.Details,
            imageIpfsCid: command.Input.ImageIpfsCid,
            submitterId: _currentPrincipal.Id,
            subjectId: command.Input.SubjectId
        );
        thing.AddEvidence(command.Input.Evidence.Select(e =>
        {
            return new Evidence(
                originUrl: e.Url,
                ipfsCid: e.HtmlIpfsCid!,
                previewImageIpfsCid: e.JpgIpfsCid!
            );
        }));
        thing.AddTags(command.Input.Tags.Select(t => t.Id));

        _thingRepository.Create(thing);

        await _thingRepository.SaveChanges();

        var thingVm = new ThingVm
        {
            Id = thing.Id!.Value
        };

        return new()
        {
            Data = new()
            {
                Thing = thingVm,
                Signature = _signer.SignThing(thingVm)
            }
        };
    }
}