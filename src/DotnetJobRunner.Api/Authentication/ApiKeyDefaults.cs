namespace DotnetJobRunner.Api.Authentication;

public static class ApiKeyDefaults
{
    public const string AuthenticationScheme = "ApiKey";
    public const string HeaderName = "X-API-Key";
    public const string QueryParameterName = "api_key";
}
