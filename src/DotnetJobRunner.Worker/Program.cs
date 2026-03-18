using DotnetJobRunner.Application;
using DotnetJobRunner.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog logging
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddHangfire(configuration =>
	configuration.UsePostgreSqlStorage(options =>
		options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

// Add health checks for monitoring
builder.Services.AddHealthChecks();

var host = builder.Build();

// Log application startup
host.Services.GetRequiredService<ILogger<Program>>()
	.LogInformation("Dotnet Job Runner Worker starting...");

await host.RunAsync();
