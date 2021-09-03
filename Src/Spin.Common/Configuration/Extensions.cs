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
                .AddJsonPath("{BaseConfigPath}/{environment}-SpinResource.json")
                .AddJsonPath($"{{BaseConfigPath}}/{{environment}}-{serviceName}.json")
                .AddJsonPath($"{{BaseConfigPath}}/{{environment}}-{serviceName}.secret.json", optional:true);
        }
    }
}
