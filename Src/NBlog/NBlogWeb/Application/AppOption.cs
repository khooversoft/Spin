using NBlog.sdk;
using Toolbox.Azure.Identity;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlogWeb.Application;

public record AppOption : UserSecretName
{
    public string? AppInsightsConnectionString { get; init; }
}
