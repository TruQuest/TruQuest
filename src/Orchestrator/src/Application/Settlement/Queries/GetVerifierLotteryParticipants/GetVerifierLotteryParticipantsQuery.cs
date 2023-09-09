using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ThingId { get; init; }
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifierLotteryParticipantsQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
        RuleFor(q => q.ProposalId).NotEmpty();
    }
}

internal class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly ISettlementProposalAssessmentVerifierLotteryEventQueryable _settlementProposalAssessmentVerifierLotteryEventQueryable;

    public GetVerifierLotteryParticipantsQueryHandler(
        ISettlementProposalAssessmentVerifierLotteryEventQueryable settlementProposalAssessmentVerifierLotteryEventQueryable
    )
    {
        _settlementProposalAssessmentVerifierLotteryEventQueryable = settlementProposalAssessmentVerifierLotteryEventQueryable;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        var (commitment, lotteryClosedEvent, participants, claimants) = await _settlementProposalAssessmentVerifierLotteryEventQueryable
            .GetOrchestratorCommitmentAndParticipants(query.ThingId, query.ProposalId);

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                OrchestratorCommitment = commitment,
                LotteryClosedEvent = lotteryClosedEvent,
                Participants = participants,
                Claimants = claimants
            }
        };
    }
}
