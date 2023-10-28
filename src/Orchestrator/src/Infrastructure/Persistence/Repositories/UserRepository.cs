using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;
using Domain.Errors;
using UserDm = Domain.Aggregates.User;
using Application.Common.Interfaces;

using Infrastructure.User;

namespace Infrastructure.Persistence.Repositories;

internal class UserRepository : Repository, IUserRepository
{
    private new readonly AppDbContext _dbContext;
    private readonly UserManager<UserDm> _userManager;

    public UserRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope,
        UserManager<UserDm> userManager
    ) : base(dbContext, sharedTxnScope)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task<UserDm?> FindById(string userId) => _userManager.FindByIdAsync(userId);

    public Task<UserDm?> FindByEmail(string email) => _userManager.FindByEmailAsync(email);

    public Task<UserDm?> FindByUsername(string username) => _userManager.FindByNameAsync(username);

    public Task<List<string>> GetUserIdsByWalletAddresses(IEnumerable<string> walletAddresses) =>
        _dbContext.UserClaims
            .Where(c => c.ClaimType == "wallet_address" && walletAddresses.Contains(c.ClaimValue))
            .Select(c => c.UserId)
            .ToListAsync();

    public async Task<UserError?> Create(UserDm user)
    {
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            // @@NOTE: Since auto-saving is enabled by default, a possible
            // error (like duplicate email/username) is detected right away.
            return new UserError(result.ToErrorDictionary());
        }

        return null;
    }

    public async Task<UserError?> AddClaimsTo(UserDm user, IList<Claim> claims)
    {
        var result = await _userManager.AddClaimsAsync(user, claims);
        if (!result.Succeeded)
        {
            return new UserError(result.ToErrorDictionary());
        }

        return null;
    }

    public async Task<Claim> GetClaim(string userId, string claimType)
    {
        var claim = await _dbContext.UserClaims
            .AsNoTracking()
            .SingleAsync(c => c.UserId == userId && c.ClaimType == claimType);

        return claim.ToClaim();
    }

    public async Task<List<Claim>> GetClaimsExcept(string userId, IEnumerable<string> except)
    {
        var claims = await _dbContext.UserClaims
            .AsNoTracking()
            .Where(c => c.UserId == userId && !except.Contains(c.ClaimType))
            .ToListAsync();

        return claims.Select(c => c.ToClaim()).ToList();
    }

    public async Task<IEnumerable<(string Id, IReadOnlyList<int>? Transports)>> GetAuthCredentialDescriptorsFor(string userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.AuthCredentials) // @@??: Is this necessary? Will 'Select' below auto-include?
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                CredentialDescriptors = u.AuthCredentials.Select(c => new { c.Id, c.Transports })
            })
            .SingleAsync();

        return user.CredentialDescriptors.Select(c => (Id: c.Id, Transports: c.Transports));
    }

    public async Task<bool> CheckCredentialIdUnique(string credentialId)
    {
        var count = await _dbContext.AuthCredentials.CountAsync(c => c.Id == credentialId);
        return count == 0;
    }

    public Task<AuthCredential?> GetAuthCredential(string credentialId) =>
        _dbContext.AuthCredentials.SingleOrDefaultAsync(c => c.Id == credentialId);
}
