using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;
using Application.Common.Monitoring;
using static Application.Common.Monitoring.LogMessagePlaceholders;

namespace Application.Settlement.Commands.FinalizeAssessmentPoll;

[ExecuteInTxn]
public class FinalizeAssessmentPollCommand : IRequest<VoidResult>
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required AssessmentDecision Decision { get; init; }
    public required string VoteAggIpfsCid { get; init; }
    public required List<string> RewardedVerifiers { get; init; }
    public required List<string> SlashedVerifiers { get; init; }

    public IEnumerable<(string Name, object? Value)> GetActivityTags(VoidResult _)
    {
        return new (string, object?)[]
        {
            (ActivityTags.ThingId, ThingId),
            (ActivityTags.SettlementProposalId, SettlementProposalId),
            (ActivityTags.IpfsCid, VoteAggIpfsCid),
            (ActivityTags.Decision, Decision.ToString())
        };
    }
}

public class FinalizeAssessmentPollCommandHandler : IRequestHandler<FinalizeAssessmentPollCommand, VoidResult>
{
    private readonly ILogger<FinalizeAssessmentPollCommandHandler> _logger;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IThingRepository _thingRepository;
    private readonly ISettlementProposalRepository _settlementProposalRepository;
    private readonly ISettlementProposalUpdateRepository _settlementProposalUpdateRepository;
    private readonly IThingUpdateRepository _thingUpdateRepository;

    public FinalizeAssessmentPollCommandHandler(
        ILogger<FinalizeAssessmentPollCommandHandler> logger,
        ISubjectRepository subjectRepository,
        IThingRepository thingRepository,
        ISettlementProposalRepository settlementProposalRepository,
        ISettlementProposalUpdateRepository settlementProposalUpdateRepository,
        IThingUpdateRepository thingUpdateRepository
    )
    {
        _logger = logger;
        _subjectRepository = subjectRepository;
        _thingRepository = thingRepository;
        _settlementProposalRepository = settlementProposalRepository;
        _settlementProposalUpdateRepository = settlementProposalUpdateRepository;
        _thingUpdateRepository = thingUpdateRepository;
    }

    public async Task<VoidResult> Handle(FinalizeAssessmentPollCommand command, CancellationToken ct)
    {
        var thing = await _thingRepository.FindById(command.ThingId);
        var proposal = await _settlementProposalRepository.FindById(command.SettlementProposalId);
        if (proposal.State != SettlementProposalState.VerifiersSelectedAndPollInitiated)
        {
            _logger.LogWarning(
                $"Trying to finalize an already finalized settlement proposal {SettlementProposalId} assessment poll",
                proposal.Id
            );
            return VoidResult.Instance;
        }

        var decision = command.Decision;

        // @@TODO!!: Handle unsettled!

        if (decision == AssessmentDecision.Accepted)
        {
            proposal.SetState(SettlementProposalState.Accepted);
            thing.AcceptSettlementProposal(proposal.Id);
            thing.SetState(ThingState.Settled);

            await _thingUpdateRepository.AddOrUpdate(new ThingUpdate(
                thingId: thing.Id,
                category: ThingUpdateCategory.General,
                updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                title: "Thing settled!",
                details: "A settlement proposal has been accepted"
            ));
            await _thingUpdateRepository.SaveChanges();

            await _subjectRepository.ContributeToRatingWithAnotherSettledThing(thing.SubjectId, proposal.Verdict);
            await _subjectRepository.SaveChanges();
        }
        else if (decision == AssessmentDecision.SoftDeclined)
        {
            proposal.SetState(SettlementProposalState.Declined);
        }
        else if (decision == AssessmentDecision.HardDeclined)
        {
            proposal.SetState(SettlementProposalState.Declined);
        }

        proposal.SetVoteAggIpfsCid(command.VoteAggIpfsCid);

        await _settlementProposalUpdateRepository.AddOrUpdate(new SettlementProposalUpdate(
            settlementProposalId: proposal.Id,
            category: SettlementProposalUpdateCategory.General,
            updateTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            title: "Proposal assessment poll completed",
            details: $"Decision: {decision}"
        ));

        await _settlementProposalRepository.SaveChanges();
        await _thingRepository.SaveChanges();
        await _settlementProposalUpdateRepository.SaveChanges();

        _logger.LogInformation(
            $"Finalized settlement proposal {SettlementProposalId} assessment poll: set appropriate state, added aggregate vote IPFS cid, etc",
            proposal.Id
        );

        return VoidResult.Instance;
    }
}
