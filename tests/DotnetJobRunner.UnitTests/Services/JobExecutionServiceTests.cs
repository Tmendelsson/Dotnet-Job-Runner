using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Services;
using DotnetJobRunner.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotnetJobRunner.UnitTests.Services;

public class JobExecutionServiceTests
{
    private readonly Mock<IJobRepository> _repository = new();
    private readonly Mock<IJobHandlerResolver> _handlerResolver = new();
    private readonly Mock<IJobExecutionLimiter> _executionLimiter = new();
    private readonly Mock<ILogger<JobExecutionService>> _logger = new();
    private readonly JobExecutionService _service;

    public JobExecutionServiceTests()
    {
        _executionLimiter
            .Setup(x => x.AcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NoOpLease());

        _service = new JobExecutionService(
            _repository.Object,
            _handlerResolver.Object,
            _executionLimiter.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Should_Complete_Job_When_Execution_Succeeds()
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = "test-job",
            Priority = JobPriority.Normal,
            Status = JobStatus.Queued,
            RetryCount = 0,
            MaxRetries = 3
        };

        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        SetupSuccessfulHandler(job.Type);

        await _service.Execute(job.Id, CancellationToken.None);

        job.Status.Should().Be(JobStatus.Completed);
        job.FinishedAt.Should().NotBeNull();
        _repository.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _repository.Verify(x => x.AddExecutionAsync(
            It.Is<JobExecution>(e => e.Status == ExecutionStatus.Completed && e.JobId == job.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Early_When_Job_Not_Found()
    {
        var jobId = Guid.NewGuid();
        _repository
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        await _service.Execute(jobId, CancellationToken.None);

        _repository.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(x => x.AddExecutionAsync(It.IsAny<JobExecution>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Early_When_Job_Is_Canceled()
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = "test-job",
            Priority = JobPriority.Normal,
            Status = JobStatus.Canceled
        };

        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        await _service.Execute(job.Id, CancellationToken.None);

        _repository.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(x => x.AddExecutionAsync(It.IsAny<JobExecution>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Set_Status_To_Failed_When_Max_Retries_Exceeded()
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = "test-job",
            Priority = JobPriority.Normal,
            Status = JobStatus.Retrying,
            RetryCount = 3,
            MaxRetries = 3
        };

        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        SetupFailingHandler(job.Type);

        await Assert.ThrowsAnyAsync<Exception>(() => _service.Execute(job.Id, CancellationToken.None));

        job.Status.Should().Be(JobStatus.Failed);
        _repository.Verify(x => x.AddExecutionAsync(
            It.Is<JobExecution>(e => e.Status == ExecutionStatus.Failed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Set_Status_To_Failed_When_Attempt_Below_Max_Retries()
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = "test-job",
            Priority = JobPriority.Normal,
            Status = JobStatus.Processing,
            RetryCount = 0,
            MaxRetries = 3
        };

        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        SetupFailingHandler(job.Type);

        await Assert.ThrowsAnyAsync<Exception>(() => _service.Execute(job.Id, CancellationToken.None));

        job.Status.Should().Be(JobStatus.Failed);
        _repository.Verify(x => x.AddExecutionAsync(
            It.Is<JobExecution>(e => e.Status == ExecutionStatus.Failed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupSuccessfulHandler(string jobType)
    {
        var handler = new Mock<IJobHandler>();
        handler.SetupGet(x => x.PayloadType).Returns(typeof(Dictionary<string, object?>));
        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<object>(),
                It.Is<JobExecutionContext>(context => context.JobType == jobType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(JobExecutionResult.Success("Test handler completed."));

        _handlerResolver
            .Setup(x => x.Resolve(jobType))
            .Returns(handler.Object);
    }

    private void SetupFailingHandler(string jobType)
    {
        var handler = new Mock<IJobHandler>();
        handler.SetupGet(x => x.PayloadType).Returns(typeof(Dictionary<string, object?>));
        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<object>(),
                It.Is<JobExecutionContext>(context => context.JobType == jobType),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("handler failed"));

        _handlerResolver
            .Setup(x => x.Resolve(jobType))
            .Returns(handler.Object);
    }

    private sealed class NoOpLease : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
