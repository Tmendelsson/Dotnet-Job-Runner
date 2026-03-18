using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Infrastructure.Persistence;
using DotnetJobRunner.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetJobRunner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection string is not configured. " +
                "Ensure it's set in appsettings.json or environment-specific settings (appsettings.Development.json, appsettings.Production.json).");

        services.AddDbContext<JobDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }
}
