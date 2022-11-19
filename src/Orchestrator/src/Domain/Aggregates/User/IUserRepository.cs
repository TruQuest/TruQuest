using System.Security.Claims;

using Domain.Base;
using Domain.Errors;

namespace Domain.Aggregates;

public interface IUserRepository : IRepository<User>
{
    Task<AccountError?> Create(User user);
    Task<AccountError?> AddClaimsTo(User user, params Claim[] claims);
}