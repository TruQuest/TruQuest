using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVotes;

public class GetVotesQuery : IRequest<HandleResult<GetVotesResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class Validator : AbstractValidator<GetVotesQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
    }
}

internal class GetVotesQueryHandler : IRequestHandler<GetVotesQuery, HandleResult<GetVotesResultVm>>
{
    private readonly ICurrentPrincipal _currentPrincipal;
    private readonly IThingValidationPollVoteQueryable _validationPollVoteQueryable;

    public GetVotesQueryHandler(
        ICurrentPrincipal currentPrincipal,
        IThingValidationPollVoteQueryable validationPollVoteQueryable
    )
    {
        _currentPrincipal = currentPrincipal;
        _validationPollVoteQueryable = validationPollVoteQueryable;
    }

    public async Task<HandleResult<GetVotesResultVm>> Handle(GetVotesQuery query, CancellationToken ct)
    {
        var (pollResult, votes) = await _validationPollVoteQueryable.GetAllFor(
            query.ThingId, _currentPrincipal.Id
        );

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                ThingState = pollResult.State,
                VoteAggIpfsCid = pollResult.VoteAggIpfsCid,
                Votes = votes
            }
        };
    }
}
