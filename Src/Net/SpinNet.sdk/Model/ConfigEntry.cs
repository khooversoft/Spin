using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Model;

public record ConfigEntry
{
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public required string Key { get; init; }
    public required string Value { get; init; }
}


public static class ConfigEntryExtensions
{
    public static bool IsValid(this ConfigEntry subject) =>
        subject != null &&
        !subject.Key.IsEmpty() &&
        !subject.Value.IsEmpty();

    public static ConfigEntry Verify(this ConfigEntry subject) => subject.Action(x => x.IsValid().Assert(x => x == true, "Invalid"));
}
