using DotnetJobRunner.Api.Authentication;
using Hangfire.Dashboard;

namespace DotnetJobRunner.Api.Authorization;

/// <summary>
/// Authorization filter for Hangfire Dashboard using the same API key credentials as the API.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin"))
        {
            return true;
        }

        var apiKey = httpContext.Request.Headers[ApiKeyDefaults.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = httpContext.Request.Query[ApiKeyDefaults.QueryParameterName].FirstOrDefault();
        }

        var validator = httpContext.RequestServices.GetRequiredService<IApiKeyValidator>();
        var principal = validator.Validate(apiKey);
        if (principal?.Claims.Any(claim => claim.Type == System.Security.Claims.ClaimTypes.Role && claim.Value == "Admin") == true)
        {
            return true;
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return false;
    }
}
