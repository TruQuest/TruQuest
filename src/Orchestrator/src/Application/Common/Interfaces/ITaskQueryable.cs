using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface ITaskQueryable
{
    Task<List<DeferredTask>> GetAllWithScheduledBlockNumber(long leBlockNumber);
}
