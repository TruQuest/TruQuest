using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;

using MediatR;

using Domain.Aggregates;

using Application.Thing.Commands.CloseValidationPoll;
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
    private readonly IServiceProvider _serviceProvider;
    private readonly ITaskQueryable _taskQueryable;

    public BlockMinedEventHandler(IServiceProvider serviceProvider, ITaskQueryable taskQueryable)
    {
        _serviceProvider = serviceProvider;
        _taskQueryable = taskQueryable;
    }

    public async Task Handle(BlockMinedEvent @event, CancellationToken ct)
    {
        var tasks = await _taskQueryable.GetAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber);
        // @@TODO??: Handle tasks in parallel ?
        foreach (var task in tasks)
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<SenderWrapper>();

            // @@TODO: Implement retry/?archive? logic.

            switch (task.Type)
            {
                case TaskType.CloseThingValidationVerifierLottery:
                    await sender.Send(new Thing.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseThingValidationPoll:
                    await sender.Send(new CloseValidationPollCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseSettlementProposalAssessmentVerifierLottery:
                    await sender.Send(new Settlement.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    });
                    break;
                case TaskType.CloseSettlementProposalAssessmentPoll:
                    await sender.Send(new CloseAssessmentPollCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
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
