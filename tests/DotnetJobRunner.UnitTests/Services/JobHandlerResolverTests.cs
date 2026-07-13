using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Services;
using FluentAssertions;
using Moq;

namespace DotnetJobRunner.UnitTests.Services;

public class JobHandlerResolverTests
{
    [Fact]
    public void Should_Resolve_Handler_By_Job_Type_Case_Insensitive()
    {
        var handler = CreateHandler("send-email");
        var resolver = new JobHandlerResolver([handler.Object]);

        var result = resolver.Resolve(" SEND-EMAIL ");

        result.Should().BeSameAs(handler.Object);
    }

    [Fact]
    public void Should_Return_True_When_Handler_Exists()
    {
        var resolver = new JobHandlerResolver([CreateHandler("generate-report").Object]);

        var exists = resolver.Exists("generate-report");

        exists.Should().BeTrue();
    }

    [Fact]
    public void Should_Return_False_When_Handler_Does_Not_Exist()
    {
        var resolver = new JobHandlerResolver([CreateHandler("import-csv").Object]);

        var exists = resolver.Exists("send-email");

        exists.Should().BeFalse();
    }

    [Fact]
    public void Should_Throw_Clear_Error_When_Handler_Does_Not_Exist()
    {
        var resolver = new JobHandlerResolver([CreateHandler("sync-customers").Object]);

        var act = () => resolver.Resolve("unknown-job");

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*unknown-job*sync-customers*");
    }

    [Fact]
    public void Should_Expose_Supported_Job_Types()
    {
        var resolver = new JobHandlerResolver(
        [
            CreateHandler("send-email").Object,
            CreateHandler("import-csv").Object
        ]);

        resolver.SupportedJobTypes.Should().BeEquivalentTo("send-email", "import-csv");
    }

    private static Mock<IJobHandler> CreateHandler(string jobType)
    {
        var handler = new Mock<IJobHandler>();
        handler.SetupGet(x => x.JobType).Returns(jobType);
        return handler;
    }
}
