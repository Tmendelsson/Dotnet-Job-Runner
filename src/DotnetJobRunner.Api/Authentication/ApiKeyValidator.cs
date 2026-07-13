using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace DotnetJobRunner.Api.Authentication;

public class ApiKeyValidator(IOptionsMonitor<ApiKeyAuthenticationOptions> options) : IApiKeyValidator
{
    public ApiKeyPrincipal? Validate(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var credential = options.CurrentValue.Keys.FirstOrDefault(key =>
            !string.IsNullOrWhiteSpace(key.Value) &&
            string.Equals(key.Value, apiKey, StringComparison.Ordinal));

        if (credential is null)
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, credential.Name)
        };

        claims.AddRange(credential.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => new Claim(ClaimTypes.Role, role)));

        return new ApiKeyPrincipal
        {
            Name = credential.Name,
            Claims = claims
        };
    }
}
