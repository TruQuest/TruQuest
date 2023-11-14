using Domain.Base;

namespace Domain.Aggregates;

public interface IDeadLetterRepository : IRepository<DeadLetter>
{
    void Create(DeadLetter deadLetter);
}
