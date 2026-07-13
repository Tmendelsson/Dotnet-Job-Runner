using System.Security.Claims;

namespace DotnetJobRunner.Api.Authentication;

public sealed class ApiKeyPrincipal
{
    public required string Name { get; init; }

    public IReadOnlyList<Claim> Claims { get; init; } = [];
}
