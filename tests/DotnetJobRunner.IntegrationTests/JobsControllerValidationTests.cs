using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotnetJobRunner.IntegrationTests;

public class JobsControllerValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public JobsControllerValidationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_CreateJobPayload_Is_Invalid()
    {
        using var client = _factory.CreateClient();

        var invalidPayload = new
        {
            type = "",
            priority = "urgent",
            maxRetries = 20,
            payload = new { }
        };

        var response = await client.PostAsJsonAsync("/jobs", invalidPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(body);
        Assert.NotNull(body!.Errors);
        Assert.Contains("Type", body.Errors.Keys);
    }
}
