using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Model;

public record Payload
{
    public required string PayloadId { get; init; }
    public string? ResourceUri { get; init; }
    public required string TypeName { get; init; }
    public string Content { get; init; } = null!;
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}

public static class NetPayloadExtensions
{
    public static bool IsValid(this Payload subject) =>
        subject != null &&
        !subject.TypeName.IsEmpty() &&
        !subject.Content.IsEmpty();

    public static Payload Verify(this Payload subject) => subject.Action(x => x.IsValid().Assert(x => x == true, "Invalid"));

    public static T ToObject<T>(this Payload subject) => subject.NotNull()
        .Content.ToObject<T>()
        .NotNull(message: "Serialization error");

    public static IReadOnlyList<T> GetTypedPayloads<T>(this IEnumerable<Payload> subjects) => subjects.NotNull()
        .GetTypedPayloads(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<Payload> GetTypedPayloads(this IEnumerable<Payload> subjects, string typeName) => subjects.NotNull()
        .Where(x => x.TypeName == typeName)
        .ToList();

    public static T? GetTypedPayloadSingle<T>(this IEnumerable<Payload> subjects) => subjects.NotNull()
        .GetTypedPayloads(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .SingleOrDefault();

}

