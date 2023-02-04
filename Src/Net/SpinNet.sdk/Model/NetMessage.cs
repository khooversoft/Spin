using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Model;

public record NetMessage
{
    public required string MessageId { get; init; }
    public required string FromId { get; init; }
    public required string ToId { get; init; }
    public required string Command { get; init; }
    public IReadOnlyList<Payload> Payloads { get; init; } = Array.Empty<Payload>();
    public IReadOnlyList<ConfigEntry> Configuration { get; init; } = new List<ConfigEntry>();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}

public static class NetHeaderExtensions
{
    public static bool IsValid(this NetMessage subject) =>
        subject != null &&
        !subject.FromId.IsEmpty() &&
        !subject.ToId.IsEmpty() &&
        subject.Payloads != null &&
        subject.Configuration != null;

    public static NetMessage Verify(this NetMessage subject) => subject.Action(x => x.IsValid().Assert(x => x == true, "Invalid"));

    public static IReadOnlyList<T> GetTypedPayloads<T>(this NetMessage subject) => subject
        .GetTypedPayloads(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static T GetTypedPayloadSingle<T>(this NetMessage subject) => subject
        .GetTypedPayloads(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .Single();

    public static IReadOnlyList<Payload> GetTypedPayloads(this NetMessage subject, string typeName) => subject.NotNull()
        .Payloads
        .Where(x => x.TypeName == typeName)
        .ToList();
}
