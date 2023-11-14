using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Thing.Commands.CloseValidationPoll;
using Application.Settlement.Commands.CloseAssessmentPoll;
using Application.Common.Misc;
using Application.Common.Interfaces;
using Application.Common.Errors;
using Application.Common.Models.IM;
using Application.General.Commands.ArchiveDeadLetter;

namespace Application.Ethereum.Events.BlockMined;

public class BlockMinedEvent : IEvent
{
    public required long BlockNumber { get; init; }
}

public class BlockMinedEventHandler : IEventHandler<BlockMinedEvent>
{
    private readonly ILogger<BlockMinedEventHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ITaskQueryable _taskQueryable;

    public BlockMinedEventHandler(
        ILogger<BlockMinedEventHandler> logger,
        IServiceScopeFactory serviceScopeFactory,
        ITaskQueryable taskQueryable
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _taskQueryable = taskQueryable;
    }

    public async Task Handle(BlockMinedEvent @event, CancellationToken ct)
    {
        var tasks = await _taskQueryable.GetAllWithScheduledBlockNumber(leBlockNumber: @event.BlockNumber);
        // @@TODO??: Handle tasks in parallel ?
        foreach (var task in tasks)
        {
            DeferredTaskCommand command;
            switch (task.Type)
            {
                case TaskType.CloseThingValidationVerifierLottery:
                    command = new Thing.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    };
                    break;
                case TaskType.CloseThingValidationPoll:
                    command = new CloseValidationPollCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    };
                    break;
                case TaskType.CloseSettlementProposalAssessmentVerifierLottery:
                    command = new Settlement.Commands.CloseVerifierLottery.CloseVerifierLotteryCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        Data = ((JsonElement)task.Payload["data"]).GetString()!.HexToByteArray(),
                        UserXorData = ((JsonElement)task.Payload["userXorData"]).GetString()!.HexToByteArray(),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    };
                    break;
                case TaskType.CloseSettlementProposalAssessmentPoll:
                    command = new CloseAssessmentPollCommand
                    {
                        Traceparent = ((JsonElement)task.Payload["traceparent"]).GetString()!,
                        ThingId = Guid.Parse(((JsonElement)task.Payload["thingId"]).GetString()!),
                        SettlementProposalId = Guid.Parse(((JsonElement)task.Payload["settlementProposalId"]).GetString()!),
                        EndBlock = task.ScheduledBlockNumber - 1,
                        TaskId = task.Id!.Value
                    };
                    break;
                default: throw new InvalidOperationException();
            }

            var dead = false;
            int maxAttempts = 3; // @@TODO: Config.
            do
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var thataway = scope.ServiceProvider.GetRequiredService<Thataway>();

                var result = await thataway.Send((IRequest<VoidResult>)command);
                if (result.Error != null)
                {
                    if (((ServerError)result.Error).IsRetryable)
                    {
                        if (++command.AttemptsMade < maxAttempts)
                        {
                            _logger.LogWarning("A retryable error occured. Retrying...");
                            await Task.Delay(500); // @@TODO: Config.
                            continue;
                        }
                    }

                    dead = true;
                }

                break;
            } while (true);

            if (dead)
            {
                _logger.LogError("An unretryable (or a retryable with max attempts exhausted) error occured. Archiving...");

                using var scope = _serviceScopeFactory.CreateScope();
                var thataway = scope.ServiceProvider.GetRequiredService<Thataway>();

                var deadLetter = new DeadLetter(
                    source: DeadLetterSource.TaskSystem,
                    archivedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                );
                deadLetter.SetPayload(new()
                {
                    ["Id"] = task.Id,
                    ["Type"] = task.Type.GetString(),
                    ["ScheduledBlockNumber"] = task.ScheduledBlockNumber,
                    ["Body"] = task.Payload
                });

                await thataway.Send(new ArchiveDeadLetterCommand
                {
                    Traceparent = command.Traceparent,
                    DeadLetter = deadLetter,
                    TaskId = task.Id
                });
            }
        }
    }
}
