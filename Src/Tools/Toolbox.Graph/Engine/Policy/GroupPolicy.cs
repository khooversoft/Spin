using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public readonly struct GroupPolicy : IEquatable<GroupPolicy>
{
    public GroupPolicy(string nameIdentifier) => NameIdentifier = nameIdentifier.NotEmpty();

    [JsonConstructor]
    public GroupPolicy(string nameIdentifier, IReadOnlyList<string> members)
    {
        NameIdentifier = nameIdentifier.NotEmpty();
        Members = members.NotNull().ToImmutableArray();
    }

    public string NameIdentifier { get; }
    public IReadOnlyList<string> Members { get; } = Array.Empty<string>();

    public override bool Equals(object? obj) => obj is GroupPolicy other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(NameIdentifier, Members);

    public bool Equals(GroupPolicy other) =>
        NameIdentifier == other.NameIdentifier &&
        Members.Count == other.Members.Count &&
        Members.SequenceEqual(other.Members);

    public static bool operator ==(GroupPolicy left, GroupPolicy right) => left.Equals(right);
    public static bool operator !=(GroupPolicy left, GroupPolicy right) => !left.Equals(right);

    public static IValidator<GroupPolicy> Validator { get; } = new Validator<GroupPolicy>()
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .RuleFor(x => x.Members).NotNull()
        .Build();
}

public static class SecurityGroupTool
{
    public static Option Validate(this GroupPolicy subject) => GroupPolicy.Validator.Validate(subject).ToOptionStatus();
}
