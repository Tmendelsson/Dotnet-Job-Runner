using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Handlers;
using DotnetJobRunner.Application.Jobs.Payloads;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Jobs.Handlers;

public class SendEmailJobHandlerTests
{
    private readonly SendEmailJobHandler _handler = new(new Mock<ILogger<SendEmailJobHandler>>().Object);

    [Fact]
    public async Task Should_Send_Email_When_Payload_Is_Valid()
    {
        var payload = new SendEmailPayload
        {
            To = "cliente@email.com",
            Subject = "Bem-vindo"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Log.Should().Contain("cliente@email.com");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Should_Fail_When_To_Is_Missing()
    {
        var payload = new SendEmailPayload
        {
            Subject = "Bem-vindo"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("'to'");
    }

    [Fact]
    public async Task Should_Fail_When_Subject_Is_Missing()
    {
        var payload = new SendEmailPayload
        {
            To = "cliente@email.com"
        };

        var result = await _handler.ExecuteAsync(payload, CreateContext(), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("'subject'");
    }

    private static JobExecutionContext CreateContext() => new()
    {
        JobId = Guid.NewGuid(),
        JobType = "send-email",
        Attempt = 1,
        StartedAt = DateTime.UtcNow,
        CreatedBy = "unit-test"
    };
}
