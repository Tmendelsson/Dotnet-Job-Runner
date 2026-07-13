using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Handlers;
using DotnetJobRunner.Application.Jobs.Payloads;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Jobs.Handlers;

public class ImportCsvJobHandlerTests
{
    private readonly ImportCsvJobHandler _handler = new(new Mock<ILogger<ImportCsvJobHandler>>().Object);

    [Fact]
    public async Task Should_Import_Csv_When_Payload_Is_Valid()
    {
        var payload = new ImportCsvPayload
        {
            FileName = "customers.csv"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Log.Should().Contain("customers.csv");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Should_Fail_When_File_Name_Is_Missing()
    {
        var payload = new ImportCsvPayload();

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("'fileName'");
    }

    private static JobExecutionContext CreateContext() => new()
    {
        JobId = Guid.NewGuid(),
        JobType = "import-csv",
        Attempt = 1,
        StartedAt = DateTime.UtcNow,
        CreatedBy = "unit-test"
    };
}
