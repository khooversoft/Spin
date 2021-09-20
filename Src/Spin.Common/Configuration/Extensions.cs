using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Toolbox.Configuration;
using Toolbox.Model;

namespace Spin.Common.Configuration
{
    public static class Extensions
    {
        public static IConfigurationBuilder AddSpin(this IConfigurationBuilder configurationBuilder, string serviceName)
        {
            return configurationBuilder
                .AddJsonFile(new ResourceId("file://{ConfigStore}/{environment}-SpinResource.json}"));
        }
    }
}
