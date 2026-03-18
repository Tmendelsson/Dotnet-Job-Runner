using DotnetJobRunner.Api.Authorization;
using DotnetJobRunner.Application;
using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Infrastructure;
using DotnetJobRunner.Infrastructure.Persistence;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    builder.Services.AddHangfire(configuration =>
        configuration.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString)));
}
else
{
    builder.Services.AddScoped<IJobScheduler, NoOpJobScheduler>();
}

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        app.Logger.LogError(exception, "Unhandled exception while processing request.");

        var genericProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(genericProblemDetails);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseRouting();

// UseAuthorization must be called before endpoint mapping
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthorizationFilter()]
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

// Apply pending migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JobDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public partial class Program;

/// <summary>
/// Testing implementation of IJobScheduler that performs no-op operations.
/// This is registered when running in "Testing" environment to avoid actual job scheduling.
/// Used by integration tests via WebApplicationFactory with UseEnvironment("Testing").
/// </summary>
internal sealed class NoOpJobScheduler : IJobScheduler
{
    public void Enqueue(Guid jobId)
    {
    }

    public void Schedule(Guid jobId, DateTime runAt)
    {
    }

    public void Delete(Guid jobId)
    {
    }

    public void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression)
    {
    }

    public void RemoveRecurring(Guid recurringJobDefinitionId)
    {
    }
}
