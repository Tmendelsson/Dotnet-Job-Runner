using Hangfire.Dashboard;

namespace DotnetJobRunner.Api.Authorization;

/// <summary>
/// Authorization filter for Hangfire Dashboard to prevent anonymous access.
/// Blocks all access except from localhost during development.
/// In production, implement proper authentication with JWT or similar.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, replace this with proper authentication
        // Example: check for JWT token, API key, or other auth mechanism
        var httpContext = context.GetHttpContext();
        
        // Allow access from localhost only (development)
        if (httpContext.Connection.RemoteIpAddress is not null && httpContext.Connection.RemoteIpAddress.IsLoopback)
        {
            return true;
        }

        // Reject all other access
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return false;
    }
}
