using System.Security.Claims;

using Domain.Base;
using Domain.Errors;
using Domain.Results;

namespace Domain.Aggregates;

public interface IUserRepository : IRepository<User>
{
    Task<UserError?> Create(User user);
    Task<UserError?> AddClaimsTo(User user, List<Claim> claims);
    Task<Either<UserError, List<Claim>>> GetClaimsFor(string id);
}