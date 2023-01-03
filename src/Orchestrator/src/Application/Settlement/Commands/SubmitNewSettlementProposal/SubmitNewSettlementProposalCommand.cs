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
    private readonly IFileArchiver _fileArchiver;
    private readonly ISettlementProposalRepository _settlementProposalRepository;

    public SubmitNewSettlementProposalCommandHandler(
        ILogger<SubmitNewSettlementProposalCommandHandler> logger,
        ICurrentPrincipal currentPrincipal,
        ISigner signer,
        IFileArchiver fileArchiver,
        ISettlementProposalRepository settlementProposalRepository
    )
    {
        _logger = logger;
        _currentPrincipal = currentPrincipal;
        _signer = signer;
        _fileArchiver = fileArchiver;
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

        await foreach (var (ipfsCid, obj, prop) in _fileArchiver.ArchiveAll(command.Input, _currentPrincipal.Id))
        {
            _logger.LogDebug("File cid is " + ipfsCid);
            var attr = prop.GetCustomAttribute<FileUrlAttribute>()!;
            if (attr.KeepOriginUrl)
            {
                prop.SetValue(obj, $"{prop.GetValue(obj)}\t{ipfsCid}");
            }
            else
            {
                prop.SetValue(obj, ipfsCid);
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
            ThingId = proposal.ThingId,
            Id = proposal.Id!.Value
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