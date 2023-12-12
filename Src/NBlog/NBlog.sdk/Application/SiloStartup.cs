using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Toolbox.Tools;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
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
