using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace Toolbox
{
    public static class ConfigurationExtensions
    {
        public static T Bind<T>(this IConfiguration configuration) where T : new()
        {
            configuration.VerifyNotNull(nameof(configuration));

            var option = new T();
            configuration.Bind(option, x => x.BindNonPublicProperties = true);
            return option;
        }
    }
}
