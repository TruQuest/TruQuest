using MediatR;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVerifiers;

public class GetVerifiersQuery : IRequest<HandleResult<GetVerifiersResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifiersQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
    }
}

internal class GetVerifiersQueryHandler : IRequestHandler<GetVerifiersQuery, HandleResult<GetVerifiersResultVm>>
{
    private readonly IThingQueryable _thingQueryable;
    private readonly IThingAcceptancePollVoteQueryable _thingAcceptancePollVoteQueryable;

    public GetVerifiersQueryHandler(
        IThingQueryable thingQueryable,
        IThingAcceptancePollVoteQueryable thingAcceptancePollVoteQueryable
    )
    {
        _thingQueryable = thingQueryable;
        _thingAcceptancePollVoteQueryable = thingAcceptancePollVoteQueryable;
    }

    public async Task<HandleResult<GetVerifiersResultVm>> Handle(GetVerifiersQuery query, CancellationToken ct)
    {
        var verifiers = await _thingQueryable.GetVerifiers(query.ThingId);
        var votes = await _thingAcceptancePollVoteQueryable.GetAllFor(query.ThingId);
        foreach (var verifier in verifiers)
        {
            verifier.Vote = votes.LastOrDefault(v => v.UserId == verifier.VerifierId);
        }

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                Verifiers = verifiers
            }
        };
    }
}