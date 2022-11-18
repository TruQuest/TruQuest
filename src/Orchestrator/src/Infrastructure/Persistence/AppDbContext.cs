using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Domain.Aggregates;

namespace Infrastructure.Persistence;

internal class AppDbContext : IdentityUserContext<User, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("truquest");
        base.OnModelCreating(modelBuilder);
    }
}