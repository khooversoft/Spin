using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GroupCollection : ICollection<GroupPolicy>, IEquatable<GroupCollection>
{
    private readonly ConcurrentDictionary<string, GroupPolicy> _groups;

    public GroupCollection() => _groups = new();

    [JsonConstructor]
    public GroupCollection(IReadOnlyList<GroupPolicy> groups) => _groups = groups.NotNull()
        .ForEach(x => x.Validate().ThrowOnError())
        .ToConcurrentDictionary(x => x.NameIdentifier);

    public GroupPolicy this[string nameIdentifier]
    {
        get => _groups[nameIdentifier];
        set => _groups[nameIdentifier] = value.Action(x => x.Validate().ThrowOnError());
    }

    public int Count => _groups.Count;
    public bool IsReadOnly => false;

    public void Add(GroupPolicy group)
    {
        group.Validate().ThrowOnError();
        _groups[group.NameIdentifier] = group;
    }

    public void Clear() => _groups.Clear();

    public bool Contains(GroupPolicy item) =>
        _groups.TryGetValue(item.NameIdentifier, out var existing) && existing == item;

    public void CopyTo(GroupPolicy[] array, int arrayIndex)
    {
        array.NotNull();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        foreach (var item in _groups.Values)
        {
            if (arrayIndex >= array.Length) throw new ArgumentException("Destination array is not long enough.");
            array[arrayIndex++] = item;
        }
    }

    public bool Remove(GroupPolicy group) => _groups.TryRemove(group.NameIdentifier, out _);

    // Convenience helpers
    public bool Contains(string nameIdentifier) => _groups.ContainsKey(nameIdentifier);
    public bool TryGetGroup(string nameIdentifier, out GroupPolicy group) => _groups.TryGetValue(nameIdentifier, out group!);

    public bool InGroup(string groupIdentifier, string principalIdentifier)
    {
        if (TryGetGroup(groupIdentifier, out var group)) return group.Members.Contains(principalIdentifier);
        return false;
    }

    public IEnumerator<GroupPolicy> GetEnumerator() => _groups.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Equality
    public override bool Equals(object? obj) => obj is GroupCollection other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_groups);

    public bool Equals(GroupCollection? other) => other is not null &&
        _groups.Count == other._groups.Count &&
        _groups.All(pair =>
            other._groups.TryGetValue(pair.Key, out var otherGroups) &&
            pair.Value == otherGroups
        );

    public static bool operator ==(GroupCollection? left, GroupCollection? right) => left?.Equals(right) ?? false;
    public static bool operator !=(GroupCollection? left, GroupCollection? right) => !left?.Equals(right) ?? false;
}
