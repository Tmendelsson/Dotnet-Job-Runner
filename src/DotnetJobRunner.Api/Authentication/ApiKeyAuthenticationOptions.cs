using Microsoft.AspNetCore.Authentication;

namespace DotnetJobRunner.Api.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = ApiKeyDefaults.HeaderName;

    public List<ApiKeyCredential> Keys { get; set; } = [];
}
