using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpinCluster.sdk.Application;

public static class SpinClusterConstants
{
    public static class Schema
    {
        public const string User = "user";
        public const string Group = "group";
        public const string Key = "principle-key";
    }

    public static class Protocol
    {
        public const string TraceId = "spin-trace-id";
    }
}
