namespace DotnetJobRunner.Api.Authentication;

public class ApiKeyCredential
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}
