using System;
using System.Collections.Generic;

namespace Spin.Common.Configuration.Model
{
    public record QueueModel
    {
        public string Channel { get; init; } = null!;

        public bool AutoComplete { get; init; }

        public int MaxConcurrentCalls { get; init; } = 10;

        public ServiceBusQueue ServiceBus { get; init; } = null!;
    }
}