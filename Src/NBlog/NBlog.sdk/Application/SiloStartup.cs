using Microsoft.Extensions.Hosting;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class SiloStartup
{
    public static ISiloBuilder AddBlogCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        DatalakeOption option = hostContext.Configuration.GetSection("Storage").Bind<DatalakeOption>();
        option.Validate(out Option v).Assert(x => x == true, $"DatalakeOption is invalid, errors={v.Error}");

        return builder;
    }
}
