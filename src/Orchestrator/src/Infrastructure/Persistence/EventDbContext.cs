using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Domain.Aggregates.Events;

namespace Infrastructure.Persistence;

public class EventDbContext : DbContext
{
    public DbSet<ActionableThingRelatedEvent> ActionableThingRelatedEvents { get; set; }
    public DbSet<PreJoinedThingSubmissionVerifierLotteryEvent> PreJoinedThingSubmissionVerifierLotteryEvents { get; set; }
    public DbSet<JoinedThingSubmissionVerifierLotteryEvent> JoinedThingSubmissionVerifierLotteryEvents { get; set; }
    public DbSet<CastedAcceptancePollVoteEvent> CastedAcceptancePollVoteEvents { get; set; }

    public DbSet<PreJoinedThingAssessmentVerifierLotteryEvent> PreJoinedThingAssessmentVerifierLotteryEvents { get; set; }
    public DbSet<JoinedThingAssessmentVerifierLotteryEvent> JoinedThingAssessmentVerifierLotteryEvents { get; set; }
    public DbSet<ThingAssessmentVerifierLotterySpotClaimedEvent> ThingAssessmentVerifierLotterySpotClaimedEvents { get; set; }
    public DbSet<CastedAssessmentPollVoteEvent> CastedAssessmentPollVoteEvents { get; set; }

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
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.Type).HasConversion<int>().IsRequired();
            builder
                .Property(e => e.Payload)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();
        });

        modelBuilder.Entity<PreJoinedThingSubmissionVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.DataHash).IsRequired();
        });

        modelBuilder.Entity<JoinedThingSubmissionVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Nonce).IsRequired();
        });

        modelBuilder.Entity<CastedAcceptancePollVoteEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Decision).HasConversion<int>().IsRequired();
            builder.Property(e => e.Reason).IsRequired(false);
        });

        modelBuilder.Entity<PreJoinedThingAssessmentVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.DataHash).IsRequired();
        });

        modelBuilder.Entity<JoinedThingAssessmentVerifierLotteryEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Nonce).IsRequired();
        });

        modelBuilder.Entity<ThingAssessmentVerifierLotterySpotClaimedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
        });

        modelBuilder.Entity<CastedAssessmentPollVoteEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.BlockNumber).IsRequired();
            builder.Property(e => e.TxnIndex).IsRequired();
            builder.Property(e => e.ThingId).IsRequired();
            builder.Property(e => e.SettlementProposalId).IsRequired();
            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.Decision).HasConversion<int>().IsRequired();
            builder.Property(e => e.Reason).IsRequired(false);
        });

        modelBuilder.Entity<BlockProcessedEvent>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.BlockNumber).IsRequired(false);
        });
    }
}