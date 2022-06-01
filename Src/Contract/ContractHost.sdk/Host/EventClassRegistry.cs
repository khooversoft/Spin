using ContractHost.sdk.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace ContractHost.sdk.Host;

public delegate Task RegistryEventNameHandler(IEventService service, IContractHost host, CancellationToken token);

public record EventClassRegistry
{
    public EventName EventName { get; init; }

    public Type Type { get; init; } = null!;

    public RegistryEventNameHandler Method { get; init; } = null!;
}


public static class EventClassRegistryExtensions
{
    public static EventClassRegistry Verify(this EventClassRegistry subject)
    {
        subject.NotNull();
        subject.EventName.Assert(x => Enum.IsDefined(typeof(EventName), x), $"{nameof(EventName)} required");
        subject.Type.NotNull();
        subject.Method.NotNull();

        return subject;
    }
}
