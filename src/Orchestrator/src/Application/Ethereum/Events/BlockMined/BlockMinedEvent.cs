using System.Text.Json;

using MediatR;

using Domain.Aggregates;

using Application.Thing.Commands.CloseVerifierLottery;

namespace Application.Ethereum.Events.BlockMined;

public class BlockMinedEvent : INotification
{
    public long BlockNumber { get; init; }
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
        var tasks = await _taskRepository.FindAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber);
        foreach (var task in tasks)
        {
            switch (task.Type)
            {
                case TaskType.CloseVerifierLottery:
                    await _mediator.Send(new CloseVerifierLotteryCommand
                    {
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = Convert.FromBase64String(((JsonElement)task.Payload["data"]).GetString()!)
                    });
                    task.SetCompleted();
                    break;
            }
        }

        await _taskRepository.SaveChanges();
    }
}