using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Domain;

namespace DotnetJobRunner.Application.Services;

internal static class RecurringJobMapper
{
    public static RecurringJobResponse Map(RecurringJobDefinition recurringJob) => new()
    {
        Id = recurringJob.Id,
        Name = recurringJob.Name,
        Type = recurringJob.Type,
        CronExpression = recurringJob.CronExpression,
        Priority = recurringJob.Priority,
        IsActive = recurringJob.IsActive,
        CreatedAt = recurringJob.CreatedAt,
        LastRunAt = recurringJob.LastRunAt,
        NextRunAt = recurringJob.NextRunAt
    };
}
