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
    public async Task Should_Retry_Failed_Job_When_Requested()
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

        var result = await _service.RetryAsync(job.Id, CancellationToken.None);

        result.Should().BeTrue();
        job.Status.Should().Be(JobStatus.Retrying);
        job.RetryCount.Should().Be(2);
        job.ErrorMessage.Should().BeNull();
        _repository.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
        _scheduler.Verify(x => x.Enqueue(job.Id), Times.Once);
    }
}
