using Domain.Base;

namespace Domain.Aggregates;

public interface IWhitelistRepository : IRepository<WhitelistEntry>
{
    void Create(WhitelistEntry entry);
    void Remove(WhitelistEntry entry);
}
