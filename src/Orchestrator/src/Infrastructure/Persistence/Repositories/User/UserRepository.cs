using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using Domain.Aggregates;
using Domain.Errors;

namespace Infrastructure.Persistence.Repositories;

internal class UserRepository : Repository<User>, IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(
        IConfiguration configuration,
        AppDbContext dbContext,
        SharedTxnScope sharedTxnScope,
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
}