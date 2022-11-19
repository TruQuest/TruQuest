using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Domain.Errors;
using Application.Common.Interfaces;

namespace Infrastructure.Persistence.Repositories;

internal class UserRepository : Repository<User>, IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope,
        UserManager<User> userManager
    ) : base(configuration, dbContext, sharedTxnScope)
    {
        _userManager = userManager;
    }

    public override ValueTask SaveChanges() => ValueTask.CompletedTask;

    public async Task<AccountError?> Create(User user)
    {
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            // @@NOTE: Since auto-saving is enabled by default, a possible
            // error (like duplicate email/username) is detected right away.
            return new AccountError(result.ToErrorDictionary());
        }

        return null;
    }

    public async Task<AccountError?> AddClaimsTo(User user, params Claim[] claims)
    {
        var result = await _userManager.AddClaimsAsync(user, claims);
        if (!result.Succeeded)
        {
            return new AccountError(result.ToErrorDictionary());
        }

        return null;
    }
}