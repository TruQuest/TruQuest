using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Domain.Aggregates;
using UserDm = Domain.Aggregates.User;

namespace Infrastructure.Persistence;

public class AppDbContext : IdentityUserContext<UserDm, string>
{
    public DbSet<AuthCredential> AuthCredentials { get; set; }
    public DbSet<DeferredTask> Tasks { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Thing> Things { get; set; }
    public DbSet<ThingValidationPollVote> ThingValidationPollVotes { get; set; }
    public DbSet<SettlementProposal> SettlementProposals { get; set; }
    public DbSet<SettlementProposalAssessmentPollVote> SettlementProposalAssessmentPollVotes { get; set; }
    public DbSet<WatchedItem> WatchList { get; set; }
    public DbSet<SubjectUpdate> SubjectUpdates { get; set; }
    public DbSet<ThingUpdate> ThingUpdates { get; set; }
    public DbSet<SettlementProposalUpdate> SettlementProposalUpdates { get; set; }
    public DbSet<WhitelistEntry> Whitelist { get; set; }
    public DbSet<DeadLetter> DeadLetters { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("truquest");
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .HasPostgresEnum<SubjectType>(schema: "truquest", name: "subject_type")
            .HasPostgresEnum<ThingState>(schema: "truquest", name: "thing_state")
            .HasPostgresEnum<SettlementProposalState>(schema: "truquest", name: "settlement_proposal_state")
            .HasPostgresEnum<Verdict>(schema: "truquest", name: "verdict")
            .HasPostgresEnum<TaskType>(schema: "truquest", name: "task_type")
            .HasPostgresEnum<WatchedItemType>(schema: "truquest", name: "watched_item_type")
            .HasPostgresEnum<WhitelistEntryType>(schema: "truquest", name: "whitelist_entry_type")
            .HasPostgresEnum<DeadLetterSource>(schema: "truquest", name: "dead_letter_source")
            .HasPostgresEnum<DeadLetterState>(schema: "truquest", name: "dead_letter_state");

        modelBuilder.Entity<UserDm>(builder =>
        {
            builder.Property(u => u.WalletAddress).IsRequired();
        });

        modelBuilder.Entity<AuthCredential>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.PublicKey).IsRequired();
            builder.Property(c => c.SignCount).IsRequired();
            builder
                .Property(c => c.Transports)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired(false);
            builder.Property(c => c.IsBackupEligible).IsRequired();
            builder.Property(c => c.IsBackedUp).IsRequired();
            builder.Property(c => c.AttestationObject).IsRequired();
            builder.Property(c => c.AttestationClientDataJSON).IsRequired();
            builder.Property(c => c.AttestationFormat).IsRequired();
            builder.Property(c => c.AddedAt).IsRequired();
            builder.Property(c => c.AaGuid).IsRequired();

            builder
                .HasOne<UserDm>()
                .WithMany(u => u.AuthCredentials)
                .HasForeignKey(c => c.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<Subject>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(s => s.SubmittedAt).IsRequired();
            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.Details).IsRequired();
            builder.Property(s => s.Type).IsRequired();
            builder.Property(s => s.ImageIpfsCid).IsRequired();
            builder.Property(s => s.CroppedImageIpfsCid).IsRequired();
            builder.Property(s => s.SettledThingsCount).IsRequired();
            builder.Property(s => s.AvgScore).IsRequired();

            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(s => s.SubmitterId)
                .IsRequired();

            builder.Metadata
                .FindNavigation(nameof(Subject.Tags))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Tag>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseIdentityAlwaysColumn();
            builder.Property(t => t.Name).IsRequired();
        });

        modelBuilder.Entity<SubjectAttachedTag>(builder =>
        {
            builder.ToTable("SubjectAttachedTags");
            builder.HasKey(t => new { t.SubjectId, t.TagId });
            builder
                .HasOne<Subject>()
                .WithMany(s => s.Tags)
                .HasForeignKey(t => t.SubjectId)
                .IsRequired();
            builder
                .HasOne<Tag>()
                .WithMany()
                .HasForeignKey(t => t.TagId)
                .IsRequired();
        });

        modelBuilder.Entity<Thing>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.State).IsRequired();
            builder.Property(t => t.SubmittedAt).IsRequired(false);
            builder.Property(t => t.Title).IsRequired();
            builder.Property(t => t.Details).IsRequired();
            builder.Property(t => t.ImageIpfsCid).IsRequired(false);
            builder.Property(t => t.CroppedImageIpfsCid).IsRequired(false);
            builder.Property(t => t.VoteAggIpfsCid).IsRequired(false);
            builder.Property(t => t.AcceptedSettlementProposalId).IsRequired(false);
            builder.Property(t => t.SettledAt).IsRequired(false);
            builder
                .Property(t => t.RelatedThings)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired(false);

            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(t => t.SubmitterId)
                .IsRequired();

            builder
                .HasOne<Subject>()
                .WithMany()
                .HasForeignKey(t => t.SubjectId)
                .IsRequired();

            builder.Metadata
                .FindNavigation(nameof(Thing.Evidence))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                .FindNavigation(nameof(Thing.Tags))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                .FindNavigation(nameof(Thing.Verifiers))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ThingEvidence>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(e => e.OriginUrl).IsRequired();
            builder.Property(e => e.IpfsCid).IsRequired();
            builder.Property(e => e.PreviewImageIpfsCid).IsRequired();

            builder
                .HasOne<Thing>()
                .WithMany(t => t.Evidence)
                .HasForeignKey("ThingId")
                .IsRequired();
        });

        modelBuilder.Entity<ThingAttachedTag>(builder =>
        {
            builder.ToTable("ThingAttachedTags");
            builder.HasKey(t => new { t.ThingId, t.TagId });
            builder
                .HasOne<Thing>()
                .WithMany(t => t.Tags)
                .HasForeignKey(t => t.ThingId)
                .IsRequired();
            builder
                .HasOne<Tag>()
                .WithMany()
                .HasForeignKey(t => t.TagId)
                .IsRequired();
        });

        modelBuilder.Entity<ThingVerifier>(builder =>
        {
            builder.ToTable("ThingVerifiers");
            builder.HasKey(tv => new { tv.ThingId, tv.VerifierId });
            builder
                .HasOne<Thing>()
                .WithMany(t => t.Verifiers)
                .HasForeignKey(tv => tv.ThingId)
                .IsRequired();
            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(tv => tv.VerifierId)
                .IsRequired();
        });

        modelBuilder.Entity<DeferredTask>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseIdentityAlwaysColumn();
            builder.Property(t => t.Type).IsRequired();
            builder.Property(t => t.ScheduledBlockNumber).IsRequired();
            builder
                .Property(t => t.Payload)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();
        });

        modelBuilder.Entity<ThingValidationPollVote>(builder =>
        {
            builder.HasKey(v => new { v.ThingId, v.VoterId });
            builder.Property(v => v.VoterWalletAddress).IsRequired();
            builder.Property(v => v.CastedAtMs).IsRequired();
            builder.Property(v => v.Decision).HasConversion<int>().IsRequired();
            builder.Property(v => v.Reason).IsRequired(false);
            builder.Property(v => v.VoterSignature).IsRequired();
            builder.Property(v => v.IpfsCid).IsRequired();

            builder
                .HasOne<ThingVerifier>()
                .WithOne()
                .HasForeignKey<ThingValidationPollVote>(v => new { v.ThingId, v.VoterId })
                .IsRequired();
        });

        modelBuilder.Entity<SettlementProposal>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.State).IsRequired();
            builder.Property(p => p.SubmittedAt).IsRequired(false);
            builder.Property(p => p.Title).IsRequired();
            builder.Property(p => p.Verdict).IsRequired();
            builder.Property(p => p.Details).IsRequired();
            builder.Property(p => p.ImageIpfsCid).IsRequired(false);
            builder.Property(p => p.CroppedImageIpfsCid).IsRequired(false);
            builder.Property(p => p.VoteAggIpfsCid).IsRequired(false);
            builder.Property(p => p.AssessmentPronouncedAt).IsRequired(false);

            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(p => p.SubmitterId)
                .IsRequired();

            builder
                .HasOne<Thing>()
                .WithMany()
                .HasForeignKey(p => p.ThingId)
                .IsRequired();

            builder.Metadata
                .FindNavigation(nameof(SettlementProposal.Evidence))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                .FindNavigation(nameof(SettlementProposal.Verifiers))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SettlementProposalEvidence>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(e => e.OriginUrl).IsRequired();
            builder.Property(e => e.IpfsCid).IsRequired();
            builder.Property(e => e.PreviewImageIpfsCid).IsRequired();

            builder
                .HasOne<SettlementProposal>()
                .WithMany(p => p.Evidence)
                .HasForeignKey("SettlementProposalId")
                .IsRequired();
        });

        modelBuilder.Entity<SettlementProposalVerifier>(builder =>
        {
            builder.ToTable("SettlementProposalVerifiers");
            builder.HasKey(pv => new { pv.SettlementProposalId, pv.VerifierId });
            builder
                .HasOne<SettlementProposal>()
                .WithMany(p => p.Verifiers)
                .HasForeignKey(pv => pv.SettlementProposalId)
                .IsRequired();
            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(pv => pv.VerifierId)
                .IsRequired();
        });

        modelBuilder.Entity<SettlementProposalAssessmentPollVote>(builder =>
        {
            builder.HasKey(v => new { v.SettlementProposalId, v.VoterId });
            builder.Property(v => v.VoterWalletAddress).IsRequired();
            builder.Property(v => v.CastedAtMs).IsRequired();
            builder.Property(v => v.Decision).HasConversion<int>().IsRequired();
            builder.Property(v => v.Reason).IsRequired(false);
            builder.Property(v => v.VoterSignature).IsRequired();
            builder.Property(v => v.IpfsCid).IsRequired();

            builder
                .HasOne<SettlementProposalVerifier>()
                .WithOne()
                .HasForeignKey<SettlementProposalAssessmentPollVote>(v => new { v.SettlementProposalId, v.VoterId })
                .IsRequired();
        });

        modelBuilder.Entity<WatchedItem>(builder =>
        {
            builder.HasKey(w => new { w.UserId, w.ItemType, w.ItemId, w.ItemUpdateCategory });
            builder.Property(w => w.ItemType);
            builder.Property(w => w.LastSeenUpdateTimestamp).IsRequired();

            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<SubjectUpdate>(builder =>
        {
            builder.HasKey(u => new { u.SubjectId, u.Category });
            builder.Property(u => u.Category).HasConversion<int>();
            builder.Property(u => u.UpdateTimestamp).IsRequired();
            builder.Property(u => u.Title).IsRequired();
            builder.Property(u => u.Details).IsRequired(false);
            builder.Property(u => u.Traceparent).IsRequired(false);
        });

        modelBuilder.Entity<ThingUpdate>(builder =>
        {
            builder.HasKey(u => new { u.ThingId, u.Category });
            builder.Property(u => u.Category).HasConversion<int>();
            builder.Property(u => u.UpdateTimestamp).IsRequired();
            builder.Property(u => u.Title).IsRequired();
            builder.Property(u => u.Details).IsRequired(false);
            builder.Property(u => u.Traceparent).IsRequired(false);
        });

        modelBuilder.Entity<SettlementProposalUpdate>(builder =>
        {
            builder.HasKey(u => new { u.SettlementProposalId, u.Category });
            builder.Property(u => u.Category).HasConversion<int>();
            builder.Property(u => u.UpdateTimestamp).IsRequired();
            builder.Property(u => u.Title).IsRequired();
            builder.Property(u => u.Details).IsRequired(false);
            builder.Property(u => u.Traceparent).IsRequired(false);
        });

        modelBuilder.Entity<WhitelistEntry>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).UseIdentityAlwaysColumn();
            builder.Property(e => e.Type).IsRequired();
            builder.Property(e => e.Value).IsRequired();
        });

        modelBuilder.Entity<DeadLetter>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).UseIdentityAlwaysColumn();
            builder.Property(l => l.Source).IsRequired();
            builder.Property(l => l.ArchivedAt).IsRequired();
            builder.Property(l => l.State).IsRequired();
            builder
                .Property(l => l.Payload)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();
        });
    }
}
