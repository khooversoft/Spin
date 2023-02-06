using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Model;
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

    public static IReadOnlyList<Payload> GetTypedPayloads(this NetMessage subject, string typeName) => subject.NotNull()
        .Payloads
        .Where(x => x.TypeName == typeName)
        .ToList();

    public static T? GetTypedPayloadSingle<T>(this NetMessage subject) => subject
        .GetTypedPayloads(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .SingleOrDefault();

    public static (NetResponse? notFound, IReadOnlyList<T> value) Find<T>(this NetMessage message, Func<T, bool> isValid) where T : class
    {
        return message.GetTypedPayloads<T>() switch
        {
            var v when v.Count == 0 => (BuildBadRequestResponse<T>("not found"), Array.Empty<T>()),
            var v when v.All(y => !isValid(y)) => (BuildBadRequestResponse<T>("is invalid"), Array.Empty<T>()),
            var v => (null, v),
        };
    }

    public static (NetResponse? notFound, T value) FindSingle<T>(this NetMessage message, Func<T, bool> isValid) where T : class
    {
        return message.GetTypedPayloadSingle<T>() switch
        {
            null => (BuildBadRequestResponse<T>("not found"), null!),
            var v when !isValid(v) => (BuildBadRequestResponse<T>("is invalid"), null!),
            var v => (null, v),
        };
    }

    private static NetResponse BuildBadRequestResponse<T>(string msg) => new NetResponse
    {
        StatusCode = HttpStatusCode.BadRequest,
        Message = $"{typeof(T).GetTypeName()} message {msg}",
    };
}
