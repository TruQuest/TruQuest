using System.Reflection;

using Microsoft.Extensions.Logging;

using MediatR;

using Domain.Results;
using Domain.Aggregates;

using Application.Common.Attributes;
using Application.Common.Interfaces;

namespace Application.Settlement.Commands.SubmitNewSettlementProposal;

[RequireAuthorization]
public class SubmitNewSettlementProposalCommand : IRequest<HandleResult<SubmitNewSettlementProposalResultVm>>
{
    public NewSettlementProposalIm Input { get; set; }
    public string Signature { get; set; }
}

internal class SubmitNewSettlementProposalCommandHandler : IRequestHandler<SubmitNewSettlementProposalCommand, HandleResult<SubmitNewSettlementProposalResultVm>>
{
    private readonly ILogger<SubmitNewSettlementProposalCommandHandler> _logger;
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISigner _signer;
    private readonly IFileFetcher _fileFetcher;
    private readonly IFileStorage _fileStorage;
    private readonly ISettlementProposalRepository _settlementProposalRepository;

    public SubmitNewSettlementProposalCommandHandler(
        ILogger<SubmitNewSettlementProposalCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileFetcher fileFetcher,
        IFileStorage fileStorage,
        ISettlementProposalRepository settlementProposalRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileFetcher = fileFetcher;
        _fileStorage = fileStorage;
        _settlementProposalRepository = settlementProposalRepository;
    }

    public async Task<HandleResult<SubmitNewSettlementProposalResultVm>> Handle(
        SubmitNewSettlementProposalCommand command, CancellationToken ct
    )
    {
        // @@TODO: Check that there isn't an already funded proposal.

        var result = _signer.RecoverFromNewSettlementProposalMessage(command.Input, command.Signature);
        if (result.IsError)
        {
            return new()
            {
                Error = result.Error
            };
        }

        // check that result.Data == _currentPrincipal.Id

        await foreach (var (filePath, obj, prop) in _fileFetcher.FetchAll(command.Input, _currentPrincipal.Id))
        {
            _logger.LogDebug("File saved to " + filePath);

            var uploadResult = await _fileStorage.Upload(filePath);
            if (uploadResult.IsError)
            {
                return new()
                {
                    Error = uploadResult.Error
                };
            }

            _logger.LogDebug("File cid is " + uploadResult.Data);

            var attr = prop.GetCustomAttribute<FileUrlAttribute>()!;
            if (attr.KeepOriginUrl)
            {
                prop.SetValue(obj, $"{prop.GetValue(obj)}\t{uploadResult.Data}");
            }
            else
            {
                prop.SetValue(obj, uploadResult.Data);
            }
        }

        var proposal = new SettlementProposal(
            thingId: command.Input.ThingId,
            title: command.Input.Title,
            verdict: (Verdict)command.Input.Verdict,
            details: command.Input.Details,
            submitterId: _currentPrincipal.Id
        );
        proposal.AddEvidence(command.Input.Evidence.Select(e =>
        {
            var index = e.Url.LastIndexOf('\t');
            return new SupportingEvidence(
                originUrl: e.Url.Substring(0, index),
                truUrl: e.Url.Substring(index + 1)
            );
        }));

        _settlementProposalRepository.Create(proposal);

        await _settlementProposalRepository.SaveChanges();

        var proposalVm = new SettlementProposalVm
        {
            ThingId = proposal.ThingId.ToString(),
            Id = proposal.Id!.Value.ToString()
        };

        return new()
        {
            Data = new()
            {
                SettlementProposal = proposalVm,
                Signature = _signer.SignSettlementProposal(proposalVm)
            }
        };
    }
}