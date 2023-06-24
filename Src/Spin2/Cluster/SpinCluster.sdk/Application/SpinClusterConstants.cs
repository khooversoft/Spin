using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpinCluster.sdk.Application;

public static class SpinClusterConstants
{
    public const string SpinStateStore = "spinStateStore";

    public static class Schema
    {
        public const string User = "user";
        public const string Group = "group";
        public const string Key = "principalKey";
        public const string Storage = "storage";
    }

    public static class Protocol
    {
        public const string TraceId = "spin-trace-id";
    }
}
