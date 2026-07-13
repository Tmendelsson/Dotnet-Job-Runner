using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DotnetJobRunner.IntegrationTests;

public class JobsControllerValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TestApiKey = "test-admin-key";
    private const string ViewerApiKey = "test-viewer-key";
    private readonly WebApplicationFactory<Program> _factory;

    public JobsControllerValidationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:ApiKey:HeaderName"] = "X-API-Key",
                    ["Authentication:ApiKey:Keys:0:Name"] = "test-admin",
                    ["Authentication:ApiKey:Keys:0:Value"] = TestApiKey,
                    ["Authentication:ApiKey:Keys:0:Roles:0"] = "Admin",
                    ["Authentication:ApiKey:Keys:0:Roles:1"] = "Operator",
                    ["Authentication:ApiKey:Keys:0:Roles:2"] = "Viewer",
                    ["Authentication:ApiKey:Keys:1:Name"] = "test-viewer",
                    ["Authentication:ApiKey:Keys:1:Value"] = ViewerApiKey,
                    ["Authentication:ApiKey:Keys:1:Roles:0"] = "Viewer"
                });
            });
        });
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_CreateJobPayload_Is_Invalid()
    {
        using var client = _factory.CreateClient();
        AddApiKey(client);

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

    [Fact]
    public async Task Should_Return_BadRequest_When_Job_Type_Is_Not_Supported()
    {
        using var client = _factory.CreateClient();
        AddApiKey(client);

        var payload = new
        {
            type = "unknown-job",
            priority = "normal",
            maxRetries = 3,
            payload = new { }
        };

        var response = await client.PostAsJsonAsync("/jobs", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(body);
        Assert.NotNull(body!.Errors);
        Assert.Contains("Type", body.Errors.Keys);
    }

    [Fact]
    public async Task Should_Return_Unauthorized_When_Api_Key_Is_Missing()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/jobs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_Forbidden_When_Role_Cannot_Create_Job()
    {
        using var client = _factory.CreateClient();
        AddApiKey(client, ViewerApiKey);

        var payload = new
        {
            type = "send-email",
            priority = "normal",
            maxRetries = 3,
            payload = new
            {
                to = "cliente@email.com",
                subject = "Bem-vindo"
            }
        };

        var response = await client.PostAsJsonAsync("/jobs", payload);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static void AddApiKey(HttpClient client, string apiKey = TestApiKey)
    {
        client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }
}
