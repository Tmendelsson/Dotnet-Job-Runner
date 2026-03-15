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
        services.AddDbContext<JobDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }
}
