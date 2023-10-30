using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Settlement.Queries.GetVotes;

public class GetVotesQuery : IRequest<HandleResult<GetVotesResultVm>>
{
    public required Guid ProposalId { get; init; }
}

internal class Validator : AbstractValidator<GetVotesQuery>
{
    public Validator()
    {
        RuleFor(q => q.ProposalId).NotEmpty();
    }
}

internal class GetVotesQueryHandler : IRequestHandler<GetVotesQuery, HandleResult<GetVotesResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly ISettlementProposalAssessmentPollVoteQueryable _settlementProposalAssessmentPollVoteQueryable;

    public GetVotesQueryHandler(
        ICurrentPrincipal currentPrincipal,
        ISettlementProposalAssessmentPollVoteQueryable settlementProposalAssessmentPollVoteQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _settlementProposalAssessmentPollVoteQueryable = settlementProposalAssessmentPollVoteQueryable;
    }

    public async Task<HandleResult<GetVotesResultVm>> Handle(GetVotesQuery query, CancellationToken ct)
    {
        var (pollResult, votes) = await _settlementProposalAssessmentPollVoteQueryable.GetAllFor(
            query.ProposalId, _currentPrincipal.Id
        );

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                ProposalState = pollResult.State,
                VoteAggIpfsCid = pollResult.VoteAggIpfsCid,
                Votes = votes
            }
        };
    }
}
