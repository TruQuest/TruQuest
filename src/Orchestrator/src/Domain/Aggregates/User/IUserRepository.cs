using System.Security.Claims;

using Domain.Base;
using Domain.Errors;

namespace Domain.Aggregates;

public interface IUserRepository : IRepository<User>
{
    Task<UserError?> Create(User user);
    Task<UserError?> AddClaimsTo(User user, params Claim[] claims);
}