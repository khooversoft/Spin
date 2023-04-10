namespace Directory.sdk.Model;

public record DirectoryOption
{
    public string HostUrl { get; set; } = "{directory.hostUrl}";
    public string ApiKey { get; set; } = "{directory.apiKey}";
}

