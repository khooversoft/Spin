using NBlog.sdk;

namespace NBlogWeb.Application;

public record AppOption : UserSecretName
{
    public string? AppInsightsConnectionString { get; init; }
}
