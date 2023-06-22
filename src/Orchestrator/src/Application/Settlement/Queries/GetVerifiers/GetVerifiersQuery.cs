using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetVerifiers;

public class GetVerifiersQuery : IRequest<HandleResult<GetVerifiersResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifiersQuery>
{
    public Validator()
    {
        RuleFor(q => q.ProposalId).NotEmpty();
    }
}

internal class GetVerifiersQueryHandler : IRequestHandler<GetVerifiersQuery, HandleResult<GetVerifiersResultVm>>
{
    private readonly ISettlementProposalQueryable _settlementProposalQueryable;
    private readonly IThingAssessmentPollVoteQueryable _thingAssessmentPollVoteQueryable;

    public GetVerifiersQueryHandler(
        ISettlementProposalQueryable settlementProposalQueryable,
        IThingAssessmentPollVoteQueryable thingAssessmentPollVoteQueryable
    )
    {
        _settlementProposalQueryable = settlementProposalQueryable;
        _thingAssessmentPollVoteQueryable = thingAssessmentPollVoteQueryable;
    }

    public async Task<HandleResult<GetVerifiersResultVm>> Handle(GetVerifiersQuery query, CancellationToken ct)
    {
        var verifiers = await _settlementProposalQueryable.GetVerifiers(query.ProposalId);
        var votes = await _thingAssessmentPollVoteQueryable.GetAllFor(query.ProposalId);
        foreach (var verifier in verifiers)
        {
            verifier.Vote = votes.LastOrDefault(v => v.UserId == verifier.VerifierId);
        }

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                Verifiers = verifiers
            }
        };
    }
}