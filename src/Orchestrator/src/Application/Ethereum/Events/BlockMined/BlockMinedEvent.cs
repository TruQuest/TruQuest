using System.Text.Json;

using MediatR;

using Domain.Aggregates;

using Application.Thing.Commands.CloseAcceptancePoll;
using Application.Settlement.Commands.CloseAssessmentPoll;
using Application.Common.Misc;

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
        // @@??: Transaction should encapsulate task as well?
        var tasks = await _taskRepository.FindAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber);
        foreach (var task in tasks)
        {
            switch (task.Type)
            {
                case TaskType.CloseThingSubmissionVerifierLottery:
                    await _mediator.Send(new Thing.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1
                    });
                    break;
                case TaskType.CloseThingAcceptancePoll:
                    await _mediator.Send(new CloseAcceptancePollCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1
                    });
                    break;
                case TaskType.CloseThingAssessmentVerifierLottery:
                    await _mediator.Send(new Settlement.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1
                    });
                    break;
                case TaskType.CloseThingSettlementProposalAssessmentPoll:
                    await _mediator.Send(new CloseAssessmentPollCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1
                    });
                    break;
            }

            task.SetCompleted();
        }

        await _taskRepository.SaveChanges();
    }
}