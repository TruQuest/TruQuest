using Domain.Base;

namespace Domain.Aggregates;

public interface IVoteRepository : IRepository<Vote>
{
    void Create(Vote vote);
}