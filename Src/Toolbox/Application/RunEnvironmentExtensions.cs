using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Application
{
    public static class RunEnvironmentExtensions
    {
        public static RunEnvironment ToEnvironment(this string subject)
        {
            Enum.TryParse(subject, true, out RunEnvironment enviornment)
                .VerifyAssert(x => x == true, $"Invalid environment {subject.ToNullIfEmpty() ?? "<none>"}");

            return enviornment;
        }

        public static string ToResourceId(this RunEnvironment subject)
        {
            return subject switch
            {
                RunEnvironment.Unknown => "unknown",
                RunEnvironment.Local => "local",
                RunEnvironment.Dev => "dev",
                RunEnvironment.PreProd => "preProd",
                RunEnvironment.Prod => "prod",

                _ => throw new InvalidOperationException(),
            } + "-congfig.json";
        }

        public static string ToResourceId(this RunEnvironment subject, string baseId)
        {
            baseId.VerifyNotEmpty(nameof(baseId));

            return baseId + "." + subject.ToResourceId();
        }

        public static bool IsLocal(this RunEnvironment subject) => subject switch
        {
            RunEnvironment.Unknown => true,
            RunEnvironment.Local => true,

            _ => false,
        };
    }
}
