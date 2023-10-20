using System.Security.Claims;

using Domain.Base;
using Domain.Errors;

namespace Domain.Aggregates;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindById(string userId);
    Task<User?> FindByEmail(string email);
    Task<UserError?> Create(User user);
    Task<UserError?> AddClaimsTo(User user, IList<Claim> claims);
    Task<IList<Claim>> GetClaimsFor(User user);
    Task<IEnumerable<(string Id, IReadOnlyList<int>? Transports)>> GetAuthCredentialDescriptorsFor(string userId);
    Task<bool> CheckCredentialIdUnique(string credentialId);
}
