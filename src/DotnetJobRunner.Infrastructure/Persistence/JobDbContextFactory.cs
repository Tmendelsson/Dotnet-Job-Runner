using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DotnetJobRunner.Infrastructure.Persistence;

public class JobDbContextFactory : IDesignTimeDbContextFactory<JobDbContext>
{
    public JobDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobDbContext>();

        // Design-time fallback connection for local development.
        var connectionString = "Host=localhost;Port=5432;Database=dotnet_job_runner;Username=dotnet;Password=dotnet";
        optionsBuilder.UseNpgsql(connectionString);

        return new JobDbContext(optionsBuilder.Options);
    }
}
