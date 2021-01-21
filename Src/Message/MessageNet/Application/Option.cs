using MessageNet.sdk.Models;
using System;
using System.Collections.Generic;
using Toolbox.Application;

namespace MessageNet.Application
{
    public record Option
    {
        public RunEnvironment RunEnvironment { get; init; }

        public IReadOnlyList<MessageNodeOption> Nodes { get; init; } = null!;

        public BusNamespaceOption BusNamespace { get; set; } = null!;
    }
}