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
    public required string ResourceUri { get; init; }
    public required string Command { get; init; }
    public IReadOnlyList<Payload> Payloads { get; init; } = Array.Empty<Payload>();
    public IReadOnlyList<KeyValuePair<string, string>> Headers { get; init; } = new List<KeyValuePair<string, string>>();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}

public static class NetHeaderExtensions
{
    public static bool IsValid(this NetMessage subject) =>
        subject != null &&
        !subject.ResourceUri.IsEmpty() &&
        subject.Payloads != null &&
        subject.Headers != null;

    public static NetMessage Verify(this NetMessage subject) => subject.Action(x => x.IsValid().Assert(x => x == true, "Invalid"));

    public static (NetResponse? notFound, IReadOnlyList<T> value) Find<T>(this NetMessage message, Func<T, bool> isValid) where T : class
    {
        return message.Payloads.GetTypedPayloads<T>() switch
        {
            var v when v.Count == 0 => (BuildBadRequestResponse<T>("not found"), Array.Empty<T>()),
            var v when v.All(y => !isValid(y)) => (BuildBadRequestResponse<T>("is invalid"), Array.Empty<T>()),
            var v => (null, v),
        };
    }

    public static (NetResponse? notFound, T value) FindSingle<T>(this NetMessage message, Func<T, bool> isValid) where T : class
    {
        return message.Payloads.GetTypedPayloadSingle<T>() switch
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
