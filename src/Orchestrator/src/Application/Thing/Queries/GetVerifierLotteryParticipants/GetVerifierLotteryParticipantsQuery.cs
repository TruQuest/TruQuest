using GoThataway;
using FluentValidation;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsQuery : IRequest<HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    public required Guid ThingId { get; init; }
}

internal class Validator : AbstractValidator<GetVerifierLotteryParticipantsQuery>
{
    public Validator()
    {
        RuleFor(q => q.ThingId).NotEmpty();
    }
}

public class GetVerifierLotteryParticipantsQueryHandler :
    IRequestHandler<GetVerifierLotteryParticipantsQuery, HandleResult<GetVerifierLotteryParticipantsResultVm>>
{
    private readonly IThingValidationVerifierLotteryEventQueryable _validationVerifierLotteryEventQueryable;

    public GetVerifierLotteryParticipantsQueryHandler(
        IThingValidationVerifierLotteryEventQueryable validationVerifierLotteryEventQueryable
    )
    {
        _validationVerifierLotteryEventQueryable = validationVerifierLotteryEventQueryable;
    }

    public async Task<HandleResult<GetVerifierLotteryParticipantsResultVm>> Handle(
        GetVerifierLotteryParticipantsQuery query, CancellationToken ct
    )
    {
        var (commitment, lotteryClosedEvent, participants) = await _validationVerifierLotteryEventQueryable
            .GetOrchestratorCommitmentAndParticipants(query.ThingId);

        return new()
        {
            Data = new()
            {
                ThingId = query.ThingId,
                OrchestratorCommitment = commitment,
                LotteryClosedEvent = lotteryClosedEvent,
                Participants = participants
            }
        };
    }
}
