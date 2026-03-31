using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Common;
using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Application.Services;
using DotnetJobRunner.Application.Validation;
using DotnetJobRunner.Domain;
using FluentAssertions;
using Moq;

namespace DotnetJobRunner.UnitTests.Services;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _repository = new();
    private readonly Mock<IJobScheduler> _scheduler = new();
    private readonly JobService _service;

    public JobServiceTests()
    {
        _service = new JobService(
            _repository.Object,
            new CreateJobRequestValidator(),
            new CreateRecurringJobRequestValidator(),
            _scheduler.Object);
    }

    [Fact]
    public async Task Should_Create_Immediate_Job_When_Request_Is_Valid()
    {
        var request = new CreateJobRequest
        {
            Type = "send-email",
            Payload = new { To = "user@example.com" },
            Priority = "normal"
        };

        _repository
            .Setup(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job job, CancellationToken _) => job);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Type.Should().Be("send-email");
        result.Status.Should().Be(JobStatus.Queued);
        _repository.Verify(x => x.AddAsync(It.Is<Job>(job => job.Type == "send-email" && job.Status == JobStatus.Queued), It.IsAny<CancellationToken>()), Times.Once);
        _scheduler.Verify(x => x.Enqueue(result.Id), Times.Once);
    }

    [Fact]
    public async Task Should_Schedule_Job_When_RunAt_Is_Provided()
    {
        var runAt = DateTime.UtcNow.AddMinutes(5);
        var request = new CreateJobRequest
        {
            Type = "generate-report",
            Payload = new { ReportName = "sales" },
            Priority = "high",
            RunAt = runAt
        };

        _repository
            .Setup(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job job, CancellationToken _) => job);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Status.Should().Be(JobStatus.Scheduled);
        result.ScheduledAt.Should().Be(runAt);
        _scheduler.Verify(x => x.Schedule(result.Id, runAt), Times.Once);
    }

    [Fact]
    public async Task Should_Return_Paged_Jobs_When_Querying_List()
    {
        var jobs = new List<Job>
        {
            new() { Id = Guid.NewGuid(), Type = "job-a", Priority = JobPriority.Normal, Status = JobStatus.Completed },
            new() { Id = Guid.NewGuid(), Type = "job-b", Priority = JobPriority.High, Status = JobStatus.Failed }
        };

        _repository
            .Setup(x => x.QueryAsync(null, null, null, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((jobs, 14));

        var result = await _service.ListAsync(new JobQueryRequest { Page = 2, PageSize = 10 }, CancellationToken.None);

        result.Should().BeOfType<PagedResult<JobResponse>>();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(14);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Should_Create_Recurring_Job_When_Request_Is_Valid()
    {
        var request = new CreateRecurringJobRequest
        {
            Name = "sync-customers-nightly",
            Type = "sync-customers",
            CronExpression = "0 2 * * *",
            Priority = "normal",
            Payload = new { Source = "erp" }
        };

        _repository
            .Setup(x => x.AddRecurringAsync(It.IsAny<RecurringJobDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecurringJobDefinition recurringJob, CancellationToken _) => recurringJob);

        var result = await _service.CreateRecurringAsync(request, CancellationToken.None);

        result.Name.Should().Be("sync-customers-nightly");
        result.IsActive.Should().BeTrue();
        _scheduler.Verify(x => x.AddOrUpdateRecurring(result.Id, "0 2 * * *"), Times.Once);
    }

    [Fact]
    public async Task Should_Retry_Failed_Job_With_Exponential_Backoff()
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = "import-csv",
            Priority = JobPriority.Normal,
            Status = JobStatus.Failed,
            RetryCount = 1,
            ErrorMessage = "temporary failure"
        };

        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _scheduler
            .Setup(x => x.Schedule(job.Id, It.IsAny<DateTime>()))
            .Returns("hangfire-retry-job-id");

        var result = await _service.RetryAsync(job.Id, CancellationToken.None);

        result.Should().Be(JobOperationResult.Success);
        job.Status.Should().Be(JobStatus.Retrying);
        job.RetryCount.Should().Be(2);
        job.ErrorMessage.Should().BeNull();
        job.HangfireJobId.Should().Be("hangfire-retry-job-id");
        job.ScheduledAt.Should().NotBeNull();
        _repository.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
        _scheduler.Verify(x => x.Schedule(job.Id, It.IsAny<DateTime>()), Times.Once);
        _scheduler.Verify(x => x.Enqueue(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Paged_Executions_When_Job_Exists()
    {
        var jobId = Guid.NewGuid();
        var job = new Job { Id = jobId, Type = "send-email", Status = JobStatus.Completed };
        var executions = new List<JobExecution>
        {
            new() { Id = Guid.NewGuid(), JobId = jobId, Attempt = 1, Status = ExecutionStatus.Completed, StartedAt = DateTime.UtcNow.AddMinutes(-5), DurationInMs = 300 },
            new() { Id = Guid.NewGuid(), JobId = jobId, Attempt = 2, Status = ExecutionStatus.Failed, StartedAt = DateTime.UtcNow.AddMinutes(-1), ErrorMessage = "timeout" }
        };

        _repository
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        _repository
            .Setup(x => x.ListExecutionsByJobIdAsync(jobId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<JobExecution>)executions, 2));

        var result = await _service.GetExecutionsAsync(jobId, 1, 20, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items[0].Status.Should().Be(ExecutionStatus.Completed);
        result.Items[1].ErrorMessage.Should().Be("timeout");
    }

    [Fact]
    public async Task Should_Return_Null_When_Job_Does_Not_Exist_For_Executions()
    {
        var jobId = Guid.NewGuid();
        _repository
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        var result = await _service.GetExecutionsAsync(jobId, 1, 20, CancellationToken.None);

        result.Should().BeNull();
        _repository.Verify(x => x.ListExecutionsByJobIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Retrying_Non_Failed_Job()
    {
        var job = new Job { Id = Guid.NewGuid(), Type = "send-email", Status = JobStatus.Completed };
        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var result = await _service.RetryAsync(job.Id, CancellationToken.None);

        result.Should().Be(JobOperationResult.InvalidState);
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Canceling_Already_Completed_Job()
    {
        var job = new Job { Id = Guid.NewGuid(), Type = "send-email", Status = JobStatus.Completed };
        _repository
            .Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var result = await _service.CancelAsync(job.Id, CancellationToken.None);

        result.Should().Be(JobOperationResult.InvalidState);
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
