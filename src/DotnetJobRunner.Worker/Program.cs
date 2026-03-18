using DotnetJobRunner.Application;
using DotnetJobRunner.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = Host.CreateDefaultBuilder(args);

// Configure Serilog logging
builder.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.ConfigureServices((context, services) =>
{
    services.AddApplication();
    services.AddInfrastructure(context.Configuration);

    var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

    services.AddHangfire(configuration =>
        configuration.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString)));
    services.AddHangfireServer();

    // Add health checks for monitoring
    services.AddHealthChecks();
});

var host = builder.Build();

// Log application startup
host.Services.GetRequiredService<ILogger<Program>>()
    .LogInformation("Dotnet Job Runner Worker starting...");

await host.RunAsync();
