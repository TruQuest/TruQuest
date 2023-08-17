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
    private readonly IThingAssessmentPollVoteQueryable _thingAssessmentPollVoteQueryable;

    public GetVotesQueryHandler(
        ICurrentPrincipal currentPrincipal,
        IThingAssessmentPollVoteQueryable thingAssessmentPollVoteQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _thingAssessmentPollVoteQueryable = thingAssessmentPollVoteQueryable;
    }

    public async Task<HandleResult<GetVotesResultVm>> Handle(GetVotesQuery query, CancellationToken ct)
    {
        var (voteAggIpfsCid, votes) = await _thingAssessmentPollVoteQueryable.GetAllFor(
            query.ProposalId, _currentPrincipal.Id
        );

        return new()
        {
            Data = new()
            {
                ProposalId = query.ProposalId,
                VoteAggIpfsCid = voteAggIpfsCid,
                Votes = votes
            }
        };
    }
}
