using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Handlers;
using DotnetJobRunner.Application.Jobs.Payloads;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Jobs.Handlers;

public class GenerateReportJobHandlerTests
{
    private readonly GenerateReportJobHandler _handler = new(new Mock<ILogger<GenerateReportJobHandler>>().Object);

    [Fact]
    public async Task Should_Generate_Report_When_Payload_Is_Valid()
    {
        var payload = new GenerateReportPayload
        {
            ReportName = "sales",
            Format = "pdf"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Log.Should().Contain("sales");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Should_Fail_When_Report_Name_Is_Missing()
    {
        var payload = new GenerateReportPayload
        {
            Format = "pdf"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("'reportName'");
    }

    private static JobExecutionContext CreateContext() => new()
    {
        JobId = Guid.NewGuid(),
        JobType = "generate-report",
        Attempt = 1,
        StartedAt = DateTime.UtcNow,
        CreatedBy = "unit-test"
    };
}
