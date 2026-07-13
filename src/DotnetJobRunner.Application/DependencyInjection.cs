using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Application.Jobs.Handlers;
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

        services.AddScoped<IJobHandlerResolver, JobHandlerResolver>();
        services.AddSingleton<IJobExecutionLimiter, InMemoryJobExecutionLimiter>();

        services.AddScoped<IJobHandler, SendEmailJobHandler>();
        services.AddScoped<IJobHandler, GenerateReportJobHandler>();
        services.AddScoped<IJobHandler, ImportCsvJobHandler>();
        services.AddScoped<IJobHandler, SyncCustomersJobHandler>();

        services.AddScoped<IValidator<CreateJobRequest>, CreateJobRequestValidator>();
        services.AddScoped<IValidator<CreateRecurringJobRequest>, CreateRecurringJobRequestValidator>();

        return services;
    }
}
