using System.Security.Claims;

using Domain.Base;
using Domain.Errors;

namespace Domain.Aggregates;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindById(string userId);
    Task<User?> FindByEmail(string email);
    Task<User?> FindByUsername(string username);
    Task<List<string>> GetUserIdsByWalletAddresses(IEnumerable<string> walletAddresses);
    Task<HandleError?> Create(User user);
    Task<HandleError?> AddClaimsTo(User user, IList<Claim> claims);
    Task<Claim> GetClaim(string userId, string claimType);
    Task<List<Claim>> GetClaimsExcept(string userId, IEnumerable<string> except);
    Task<IEnumerable<(string Id, IReadOnlyList<int>? Transports)>> GetAuthCredentialDescriptorsFor(string userId);
    Task<bool> CheckCredentialIdUnique(string credentialId);
    Task<AuthCredential?> GetAuthCredential(string credentialId);
}
