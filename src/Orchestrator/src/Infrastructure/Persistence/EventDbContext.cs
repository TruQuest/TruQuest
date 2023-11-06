using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence;

public class EventDbContext : DbContext
{
    public DbSet<ActionableThingRelatedEvent> ActionableThingRelatedEvents { get; set; }

    public DbSet<ThingValidationVerifierLotteryInitializedEvent> ThingValidationVerifierLotteryInitializedEvents { get; set; }
    public DbSet<JoinedThingValidationVerifierLotteryEvent> JoinedThingValidationVerifierLotteryEvents { get; set; }
    public DbSet<CastedThingValidationPollVoteEvent> CastedThingValidationPollVoteEvents { get; set; }

    public DbSet<SettlementProposalAssessmentVerifierLotteryInitializedEvent> SettlementProposalAssessmentVerifierLotteryInitializedEvents { get; set; }
    public DbSet<JoinedSettlementProposalAssessmentVerifierLotteryEvent> JoinedSettlementProposalAssessmentVerifierLotteryEvents { get; set; }
    public DbSet<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent> ClaimedSettlementProposalAssessmentVerifierLotterySpotEvents { get; set; }
    public DbSet<CastedSettlementProposalAssessmentPollVoteEvent> CastedSettlementProposalAssessmentPollVoteEvents { get; set; }

    public DbSet<BlockProcessedEvent> BlockProcessedEvent { get; set; }

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("truquest_events");

        modelBuilder.Entity<ActionableThingRelatedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.Type).HasConversion<int>().IsRequired();
            builder
                .Property(e => e.Payload)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.HasIndex(e => e.TxnHash).IsUnique();
        });

        modelBuilder.Entity<ThingValidationVerifierLotteryInitializedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.L1BlockNumber).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.DataHash).IsRequired();
            builder.Property(e => e.UserXorDataHash).IsRequired();

            builder.HasIndex(e => e.TxnHash).IsUnique();
        });

        modelBuilder.Entity<JoinedThingValidationVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.LogIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.WalletAddress).IsRequired();
            builder.Property(e => e.L1BlockNumber).IsRequired();
            builder.Property(e => e.UserData).IsRequired();
            builder.Property(e => e.Nonce).IsRequired(false);

            // @@NOTE: Because of AA multiple users can join in the same transaction, meaning,
            // making a unique index on just TxnHash could lead to incorrect duplicate detection.
            builder.HasIndex(e => new { e.TxnHash, e.LogIndex }).IsUnique();
        });

        modelBuilder.Entity<CastedThingValidationPollVoteEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.LogIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.WalletAddress).IsRequired();
            builder.Property(e => e.Decision).HasConversion<int>().IsRequired();
            builder.Property(e => e.Reason).IsRequired(false);
            builder.Property(e => e.L1BlockNumber).IsRequired();

            builder.HasIndex(e => new { e.TxnHash, e.LogIndex }).IsUnique();
        });

        modelBuilder.Entity<SettlementProposalAssessmentVerifierLotteryInitializedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.L1BlockNumber).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.DataHash).IsRequired();
            builder.Property(e => e.UserXorDataHash).IsRequired();

            builder.HasIndex(e => e.TxnHash).IsUnique();
        });

        modelBuilder.Entity<JoinedSettlementProposalAssessmentVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.LogIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.WalletAddress).IsRequired();
            builder.Property(e => e.L1BlockNumber).IsRequired();
            builder.Property(e => e.UserData).IsRequired();
            builder.Property(e => e.Nonce).IsRequired(false);

            builder.HasIndex(e => new { e.TxnHash, e.LogIndex }).IsUnique();
        });

        modelBuilder.Entity<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.LogIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.WalletAddress).IsRequired();
            builder.Property(e => e.L1BlockNumber).IsRequired();
            builder.Property(e => e.UserData).IsRequired();
            builder.Property(e => e.Nonce).IsRequired(false);

            builder.HasIndex(e => new { e.TxnHash, e.LogIndex }).IsUnique();
        });

        modelBuilder.Entity<CastedSettlementProposalAssessmentPollVoteEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.TxnHash).IsRequired();
            builder.Property(e => e.LogIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired(false);
            builder.Property(e => e.WalletAddress).IsRequired();
            builder.Property(e => e.Decision).HasConversion<int>().IsRequired();
            builder.Property(e => e.Reason).IsRequired(false);
            builder.Property(e => e.L1BlockNumber).IsRequired();

            builder.HasIndex(e => new { e.TxnHash, e.LogIndex }).IsUnique();
        });

        modelBuilder.Entity<BlockProcessedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.BlockNumber).IsRequired(false);
        });
    }
}
