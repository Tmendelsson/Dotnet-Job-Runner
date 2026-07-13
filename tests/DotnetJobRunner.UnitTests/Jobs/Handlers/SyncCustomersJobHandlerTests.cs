using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Handlers;
using DotnetJobRunner.Application.Jobs.Payloads;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Jobs.Handlers;

public class SyncCustomersJobHandlerTests
{
    private readonly SyncCustomersJobHandler _handler = new(new Mock<ILogger<SyncCustomersJobHandler>>().Object);

    [Fact]
    public async Task Should_Sync_Customers_When_Payload_Is_Valid()
    {
        var payload = new SyncCustomersPayload
        {
            Source = "erp"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Log.Should().Contain("erp");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Should_Fail_When_Source_Is_Missing()
    {
        var payload = new SyncCustomersPayload();

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("'source'");
    }

    private static JobExecutionContext CreateContext() => new()
    {
        JobId = Guid.NewGuid(),
        JobType = "sync-customers",
        Attempt = 1,
        StartedAt = DateTime.UtcNow,
        CreatedBy = "unit-test"
    };
}
