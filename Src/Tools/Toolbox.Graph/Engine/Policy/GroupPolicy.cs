using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GroupPolicy : IEquatable<GroupPolicy>
{
    private readonly FrozenSet<string> _members;

    public GroupPolicy(string nameIdentifier)
    {
        NameIdentifier = nameIdentifier.NotEmpty();
        _members = FrozenSet<string>.Empty;
    }

    public GroupPolicy(string nameIdentifier, IEnumerable<string> members)
    {
        NameIdentifier = nameIdentifier.NotEmpty();
        _members = members.NotNull().ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    // Name of group is PK
    public string NameIdentifier { get; init; }

    public IReadOnlyCollection<string> Members
    {
        get => _members;
        init => _members = value.NotNull().ToFrozenSet<string>();
    }

    public bool IsMember(string principalIdentifier) => Members.Contains(principalIdentifier.NotEmpty());

    public bool Equals(GroupPolicy? other)
    {
        if (other is null) return false;
        if (!string.Equals(NameIdentifier, other.NameIdentifier, StringComparison.Ordinal)) return false;

        var left = Members ?? FrozenSet<string>.Empty;
        var right = other.Members ?? FrozenSet<string>.Empty;

        if (ReferenceEquals(left, right)) return true;
        if (left.Count != right.Count) return false;

        // Set equality (order-insensitive), O(n) with FrozenSet
        foreach (var item in left)
        {
            if (!right.Contains(item)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is GroupPolicy other && Equals(other);

    public override int GetHashCode()
    {
        int nameHash = StringComparer.Ordinal.GetHashCode(NameIdentifier ?? string.Empty);

        // Order-insensitive aggregation for members hash
        int membersHash = 0;
        var set = Members;
        if (set is not null)
        {
            foreach (var m in set)
            {
                membersHash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(m);
            }
        }

        return HashCode.Combine(nameHash, membersHash);
    }

    public static bool operator ==(GroupPolicy left, GroupPolicy right) => left.Equals(right);
    public static bool operator !=(GroupPolicy left, GroupPolicy right) => !(left == right);

    public static IValidator<GroupPolicy> Validator { get; } = new Validator<GroupPolicy>()
        .RuleFor(x => x.NameIdentifier).NotEmpty()
        .RuleFor(x => x.Members).NotNull()
        .Build();
}

public static class SecurityGroupTool
{
    public static Option Validate(this GroupPolicy subject) => GroupPolicy.Validator.Validate(subject).ToOptionStatus();

    public static GroupPolicy Append(this GroupPolicy subject, string principalIdentifier)
    {
        principalIdentifier = principalIdentifier.NotEmpty();
        if (subject.Members.Contains(principalIdentifier)) return subject;

        var newMembers = subject.Members.Append(principalIdentifier).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        return subject with { Members = newMembers };
    }
}
