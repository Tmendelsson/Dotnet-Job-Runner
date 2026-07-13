namespace DotnetJobRunner.Api.Authentication;

public interface IApiKeyValidator
{
    ApiKeyPrincipal? Validate(string? apiKey);
}
