using System.Text.Json;

using MediatR;

using Domain.Aggregates;

using Application.Thing.Commands.CloseAcceptancePoll;
using Application.Settlement.Commands.CloseAssessmentPoll;

namespace Application.Ethereum.Events.BlockMined;

public class BlockMinedEvent : INotification
{
    public required long BlockNumber { get; init; }
}

internal class BlockMinedEventHandler : INotificationHandler<BlockMinedEvent>
{
    private readonly ISender _mediator;
    private readonly ITaskRepository _taskRepository;

    public BlockMinedEventHandler(ISender mediator, ITaskRepository taskRepository)
    {
        _mediator = mediator;
        _taskRepository = taskRepository;
    }

    public async Task Handle(BlockMinedEvent @event, CancellationToken ct)
    {
        var tasks = await _taskRepository.FindAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber - 1);
        foreach (var task in tasks)
        {
            switch (task.Type)
            {
                case TaskType.CloseThingSubmissionVerifierLottery:
                    await _mediator.Send(new Thing.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        LatestIncludedBlockNumber = task.ScheduledBlockNumber,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = Convert.FromBase64String(((JsonElement)task.Payload["data"]).GetString()!)
                    });
                    break;
                case TaskType.CloseThingAcceptancePoll:
                    await _mediator.Send(new CloseAcceptancePollCommand
                    {
                        LatestIncludedBlockNumber = task.ScheduledBlockNumber,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!)
                    });
                    break;
                case TaskType.CloseThingAssessmentVerifierLottery:
                    await _mediator.Send(new Settlement.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        LatestIncludedBlockNumber = task.ScheduledBlockNumber,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        Data = Convert.FromBase64String(((JsonElement)task.Payload["data"]).GetString()!)
                    });
                    break;
                case TaskType.CloseThingSettlementProposalAssessmentPoll:
                    await _mediator.Send(new CloseAssessmentPollCommand
                    {
                        LatestIncludedBlockNumber = task.ScheduledBlockNumber,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                    });
                    break;
            }

            task.SetCompleted();
        }

        await _taskRepository.SaveChanges();
    }
}