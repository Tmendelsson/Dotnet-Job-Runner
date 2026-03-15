using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Application.Services;
using DotnetJobRunner.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetJobRunner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<JobExecutionService>();
        services.AddScoped<RecurringJobExecutionService>();
        services.AddScoped<IValidator<CreateJobRequest>, CreateJobRequestValidator>();
        services.AddScoped<IValidator<CreateRecurringJobRequest>, CreateRecurringJobRequestValidator>();

        return services;
    }
}
