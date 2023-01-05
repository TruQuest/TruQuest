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

        await foreach (var (ipfsCid, extraIpfsCid, obj, prop) in _fileArchiver.ArchiveAll(command.Input, _currentPrincipal.Id))
        {
            _logger.LogDebug("File cid is " + ipfsCid);

            var attr = prop.GetCustomAttribute<FileUrlAttribute>()!;
            var backingProp = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(p => p.Name == attr.BackingField);
            backingProp.SetValue(obj, ipfsCid);

            if (attr is WebPageUrlAttribute webAttr && extraIpfsCid != null)
            {
                var extraBackingProp = obj.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(p => p.Name == webAttr.ExtraBackingField);
                extraBackingProp.SetValue(obj, extraIpfsCid);
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
            return new SupportingEvidence(
                originUrl: e.Url,
                ipfsCid: e.HtmlIpfsCid,
                previewImageIpfsCid: e.JpgIpfsCid
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