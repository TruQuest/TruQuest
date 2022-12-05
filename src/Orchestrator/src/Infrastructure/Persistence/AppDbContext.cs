using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Domain.Aggregates;
using UserDm = Domain.Aggregates.User;

namespace Infrastructure.Persistence;

public class AppDbContext : IdentityUserContext<UserDm, string>
{
    public DbSet<DeferredTask> Tasks { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Thing> Things { get; set; }
    public DbSet<Vote> Votes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("truquest");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subject>(builder =>
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.Details).IsRequired();
            builder.Property(s => s.Type).IsRequired();
            builder.Property(s => s.ImageUrl).IsRequired(false);

            builder
                .HasOne<UserDm>()
                .WithMany()
                .HasForeignKey(s => s.SubmitterId)
                .IsRequired();

            builder.Metadata
                .FindNavigation(nameof(Subject.Tags))
                !.SetPropertyAccessMode(PropertyAccessMode.Field);
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
            builder.Property(t => t.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(t => t.IdHash)
                .HasValueGenerator<KeccakSha3Generator>()
                .IsRequired();
            builder.Property(t => t.State).HasConversion<int>().IsRequired();
            builder.Property(t => t.Title).IsRequired();
            builder.Property(t => t.Details).IsRequired();
            builder.Property(t => t.ImageUrl).IsRequired(false);

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
                .FindNavigation(nameof(Thing.Evidence))
                !.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                .FindNavigation(nameof(Thing.Tags))
                !.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                .FindNavigation(nameof(Thing.Verifiers))
                !.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(t => t.IdHash);
        });

        modelBuilder.Entity<Evidence>(builder =>
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasValueGenerator<GuidValueGenerator>();
            builder.Property(e => e.OriginUrl).IsRequired();
            builder.Property(e => e.TruUrl).IsRequired();

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
            builder.Property(t => t.Type).HasConversion<int>().IsRequired();
            builder.Property(t => t.ScheduledBlockNumber).IsRequired();
            builder
                .Property(t => t.Payload)
                .HasColumnType("jsonb")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();
        });

        modelBuilder.Entity<Vote>(builder =>
        {
            builder.HasKey(v => new { v.ThingId, v.VoterId });
            builder.Property(v => v.PollType).HasConversion<int>().IsRequired();
            builder.Property(v => v.CastedAtMs).IsRequired();
            builder.Property(v => v.Decision).HasConversion<int>().IsRequired();
            builder.Property(v => v.Reason).IsRequired(false);
            builder.Property(v => v.VoterSignature).IsRequired();
            builder.Property(v => v.IpfsCid).IsRequired();

            builder
                .HasOne<ThingVerifier>()
                .WithMany()
                .HasForeignKey(v => new { v.ThingId, v.VoterId })
                .IsRequired();
        });
    }
}