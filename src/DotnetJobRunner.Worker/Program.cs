using DotnetJobRunner.Application;
using DotnetJobRunner.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddHangfire(configuration =>
	configuration.UsePostgreSqlStorage(options =>
		options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();

var host = builder.Build();
await host.RunAsync();
