using GoThataway;

using Domain.Aggregates;
using Domain.Results;

using Application.Common.Attributes;

namespace Application.General.Commands.ArchiveDeadLetter;

[ExecuteInTxn]
public class ArchiveDeadLetterCommand : IRequest<VoidResult>
{
    public string? Traceparent { get; init; }
    public required DeadLetter DeadLetter { get; init; }
    public long? TaskId { get; init; }
}

public class ArchiveDeadLetterCommandHandler : IRequestHandler<ArchiveDeadLetterCommand, VoidResult>
{
    private readonly IDeadLetterRepository _deadLetterRepository;
    private readonly ITaskRepository _taskRepository;

    public ArchiveDeadLetterCommandHandler(
        IDeadLetterRepository deadLetterRepository,
        ITaskRepository taskRepository
    )
    {
        _deadLetterRepository = deadLetterRepository;
        _taskRepository = taskRepository;
    }

    public async Task<VoidResult> Handle(ArchiveDeadLetterCommand command, CancellationToken ct)
    {
        _deadLetterRepository.Create(command.DeadLetter);
        await _deadLetterRepository.SaveChanges();
        if (command.TaskId != null)
        {
            await _taskRepository.SetCompletedStateFor(command.TaskId.Value);
            await _taskRepository.SaveChanges();
        }

        return VoidResult.Instance;
    }
}
