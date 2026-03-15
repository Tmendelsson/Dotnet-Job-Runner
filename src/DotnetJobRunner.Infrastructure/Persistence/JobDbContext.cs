using DotnetJobRunner.Domain;
using Microsoft.EntityFrameworkCore;

namespace DotnetJobRunner.Infrastructure.Persistence;

public class JobDbContext(DbContextOptions<JobDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<RecurringJobDefinition> RecurringJobs => Set<RecurringJobDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("text").IsRequired();
            builder.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Status).HasConversion<string>().IsRequired();
            builder.Property(x => x.ErrorMessage).HasColumnType("text");
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<RecurringJobDefinition>(builder =>
        {
            builder.ToTable("RecurringJobs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CronExpression).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("text").IsRequired();
            builder.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.IsActive);
        });
    }
}
