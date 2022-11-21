using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

using Domain.Aggregates;

namespace Infrastructure.Persistence;

internal class AppDbContext : IdentityUserContext<User, string>
{
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Tag> Tags { get; set; }

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
            builder.Property(s => s.ImageURL).IsRequired(false);

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
    }
}