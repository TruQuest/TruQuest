using System.Text.Json;

using MediatR;

using Domain.Aggregates;

using Application.Thing.Commands.CloseAcceptancePoll;
using Application.Settlement.Commands.CloseAssessmentPoll;
using Application.Common.Misc;
using Application.Common.Interfaces;

namespace Application.Ethereum.Events.BlockMined;

public class BlockMinedEvent : INotification
{
    public required long BlockNumber { get; init; }
}

internal class BlockMinedEventHandler : INotificationHandler<BlockMinedEvent>
{
    private readonly SenderWrapper _sender;
    private readonly ITaskQueryable _taskQueryable;

    public BlockMinedEventHandler(SenderWrapper sender, ITaskQueryable taskQueryable)
    {
        _sender = sender;
        _taskQueryable = taskQueryable;
    }

    public async Task Handle(BlockMinedEvent @event, CancellationToken ct)
    {
        var tasks = await _taskQueryable.GetAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber);
        // @@TODO??: Handle tasks in parallel ?
        foreach (var task in tasks)
        {
            switch (task.Type)
            {
                case TaskType.CloseThingSubmissionVerifierLottery:
                    await _sender.Send(new Thing.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseThingAcceptancePoll:
                    await _sender.Send(new CloseAcceptancePollCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseThingAssessmentVerifierLottery:
                    await _sender.Send(new Settlement.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseThingSettlementProposalAssessmentPoll:
                    await _sender.Send(new CloseAssessmentPollCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
            }
        }
    }
}
