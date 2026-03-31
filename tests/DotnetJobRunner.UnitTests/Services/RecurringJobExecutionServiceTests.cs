using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Services;
using DotnetJobRunner.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Services;

public class RecurringJobExecutionServiceTests
{
    private readonly Mock<IJobRepository> _repository = new();
    private readonly Mock<IJobScheduler> _scheduler = new();
    private readonly Mock<ILogger<RecurringJobExecutionService>> _logger = new();
    private readonly RecurringJobExecutionService _service;

    public RecurringJobExecutionServiceTests()
    {
        _service = new RecurringJobExecutionService(_repository.Object, _scheduler.Object, _logger.Object);
    }

    [Fact]
    public async Task Should_Create_And_Enqueue_Job_When_Recurring_Is_Active()
    {
        var recurring = new RecurringJobDefinition
        {
            Id = Guid.NewGuid(),
            Name = "daily-sync",
            Type = "sync-customers",
            CronExpression = "0 2 * * *",
            Priority = JobPriority.Normal,
            IsActive = true,
            MaxRetries = 3
        };

        _repository
            .Setup(x => x.GetRecurringByIdAsync(recurring.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recurring);
        _repository
            .Setup(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job job, CancellationToken _) => job);

        await _service.Execute(recurring.Id, CancellationToken.None);

        _repository.Verify(x => x.AddAsync(
            It.Is<Job>(j => j.Type == "sync-customers" && j.Status == JobStatus.Queued && j.MaxRetries == 3),
            It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(x => x.UpdateRecurringAsync(
            It.Is<RecurringJobDefinition>(r => r.LastRunAt != null),
            It.IsAny<CancellationToken>()), Times.Once);
        _scheduler.Verify(x => x.Enqueue(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Should_Skip_When_Recurring_Is_Not_Active()
    {
        var recurring = new RecurringJobDefinition
        {
            Id = Guid.NewGuid(),
            Name = "daily-sync",
            Type = "sync-customers",
            IsActive = false
        };

        _repository
            .Setup(x => x.GetRecurringByIdAsync(recurring.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recurring);

        await _service.Execute(recurring.Id, CancellationToken.None);

        _repository.Verify(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
        _scheduler.Verify(x => x.Enqueue(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Should_Skip_When_Recurring_Not_Found()
    {
        var id = Guid.NewGuid();
        _repository
            .Setup(x => x.GetRecurringByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecurringJobDefinition?)null);

        await _service.Execute(id, CancellationToken.None);

        _repository.Verify(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
        _scheduler.Verify(x => x.Enqueue(It.IsAny<Guid>()), Times.Never);
    }
}
