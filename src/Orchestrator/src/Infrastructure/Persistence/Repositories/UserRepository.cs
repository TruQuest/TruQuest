using System.Security.Claims;

using Microsoft.AspNetCore.Identity;

using Domain.Aggregates;
using Domain.Errors;
using UserDm = Domain.Aggregates.User;
using Application.Common.Interfaces;

using Infrastructure.User;

namespace Infrastructure.Persistence.Repositories;

internal class UserRepository : Repository, IUserRepository
{
    private readonly UserManager<UserDm> _userManager;

    public UserRepository(
        AppDbContext dbContext,
        ISharedTxnScope sharedTxnScope,
        UserManager<UserDm> userManager
    ) : base(dbContext, sharedTxnScope)
    {
        _userManager = userManager;
    }

    public Task<UserDm?> FindById(string userId) => _userManager.FindByIdAsync(userId);

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

    public Task<IList<Claim>> GetClaimsFor(UserDm user) => _userManager.GetClaimsAsync(user);
}
