using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Test.Application
{
    public record TestOption
    {
        public RunEnvironment RunEnvironment { get; init; }

        public IReadOnlyList<MessageNodeOption> Nodes { get; init; } = null!;

        public BusNamespaceOption BusNamespace { get; set; } = null!;
    }
}
