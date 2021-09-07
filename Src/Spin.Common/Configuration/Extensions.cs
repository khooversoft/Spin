using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Toolbox.Configuration;

namespace Spin.Common.Configuration
{
    public static class Extensions
    {
        public static IConfigurationBuilder AddSpin(this IConfigurationBuilder configurationBuilder, string serviceName)
        {
            return configurationBuilder
                .AddJsonPath("{ConfigStore}/{environment}-SpinResource.json")
                .AddJsonPath($"{{ConfigStore}}/{{environment}}-{serviceName}.json")
                .AddJsonPath($"{{ConfigStore}}/{{environment}}-{serviceName}.secret.json", optional:true);
        }
    }
}
