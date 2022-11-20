using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;

using Application.Common.Interfaces;
using Application.Common.Attributes;

namespace Application.Subject.Commands.AddNewSubject;

[RequireAuthorization]
public class AddNewSubjectCommand : IRequest<VoidResult>
{
    public NewSubjectIM Input { get; set; }
    public string Signature { get; set; }
}

internal class AddNewSubjectCommandHandler : IRequestHandler<AddNewSubjectCommand, VoidResult>
{
    private readonly ILogger<AddNewSubjectCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileFetcher _fileFetcher;

    public AddNewSubjectCommandHandler(
        ILogger<AddNewSubjectCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileFetcher fileFetcher
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileFetcher = fileFetcher;
    }

    public async Task<VoidResult> Handle(AddNewSubjectCommand command, CancellationToken ct)
    {
        var result = _signer.RecoverFromNewSubjectMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id

        await foreach (var filePath in _fileFetcher.FetchAll(command.Input, _currentPrincipal.Id))
        {
            _logger.LogDebug("File saved to " + filePath);
        }

        return VoidResult.Instance;
    }
}