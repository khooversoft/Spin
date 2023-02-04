using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Model;

public record Payload
{
    public required string PayloadId { get; set; }
    public required string TypeName { get; init; }
    public string Data { get; init; } = null!;
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}

public static class NetPayloadExtensions
{
    public static bool IsValid(this Payload subject) =>
        subject != null &&
        !subject.TypeName.IsEmpty() &&
        !subject.Data.IsEmpty();

    public static Payload Verify(this Payload subject) => subject.Action(x => x.IsValid().Assert(x => x == true, "Invalid"));

    public static T ToObject<T>(this Payload subject) => subject.NotNull()
        .Data.ToObject<T>()
        .NotNull(message: "Serialization error");
}

