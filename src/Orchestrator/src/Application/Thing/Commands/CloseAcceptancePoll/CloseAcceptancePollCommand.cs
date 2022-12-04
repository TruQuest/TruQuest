using MediatR;

using Domain.Results;

namespace Application.Thing.Commands.CloseAcceptancePoll;

public class CloseAcceptancePollCommand : IRequest<VoidResult>
{
    public long LatestIncludedBlockNumber { get; init; }
    public Guid ThingId { get; init; }
}

internal class CloseAcceptancePollCommandHandler : IRequestHandler<CloseAcceptancePollCommand, VoidResult>
{

    public async Task<VoidResult> Handle(CloseAcceptancePollCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}