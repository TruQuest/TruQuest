using Domain.Aggregates;

namespace Application.Common.Interfaces;

public interface IWhitelistQueryable
{
    Task<bool> CheckIsWhitelisted(WhitelistEntryType entryType, string value);
    Task<bool> CheckIsWhitelisted(string userId);
}
