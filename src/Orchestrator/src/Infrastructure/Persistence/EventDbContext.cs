using Microsoft.EntityFrameworkCore;

using Domain.QM;
using Domain.Aggregates.Events;

namespace Infrastructure.Persistence;

public class EventDbContext : DbContext
{
    public DbSet<ThingFundedEvent> ThingFundedEvents { get; set; }
    public DbSet<PreJoinedVerifierLotteryEvent> PreJoinedVerifierLotteryEvents { get; set; }
    public DbSet<JoinedVerifierLotteryEvent> JoinedVerifierLotteryEvents { get; set; }

    public DbSet<VerifierLotteryWinnerQm> VerifierLotteryWinners { get; set; }

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("truquest_events");

        modelBuilder.Entity<ThingFundedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.ThingIdHash).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Stake).IsRequired();
            builder.Property(e => e.Processed).IsRequired();
        });

        modelBuilder.Entity<PreJoinedVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingIdHash).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.DataHash).IsRequired();
        });

        modelBuilder.Entity<JoinedVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingIdHash).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Nonce).IsRequired();
        });

        modelBuilder.Entity<VerifierLotteryWinnerQm>(builder =>
        {
            builder.HasNoKey();
            builder.ToView(nameof(VerifierLotteryWinners));
        });
    }
}