using System.Collections;
using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GroupCollection : ICollection<GroupPolicy>, IEquatable<GroupCollection>
{
    private readonly ConcurrentDictionary<string, GroupPolicy> _groups;

    public GroupCollection() => _groups = new(StringComparer.OrdinalIgnoreCase);

    public GroupCollection(IEnumerable<GroupPolicy> groups) => _groups = groups.NotNull()
        .ForEach(x => x.Validate().ThrowOnError())
        .ToConcurrentDictionary(x => x.NameIdentifier, StringComparer.OrdinalIgnoreCase);

    public GroupPolicy this[string nameIdentifier]
    {
        get => _groups[nameIdentifier];
        set
        {
            value.Validate().ThrowOnError();
            if (value.NameIdentifier != nameIdentifier)
            {
                throw new ArgumentException($"GroupPolicy NameIdentifier '{value.NameIdentifier}' does not match indexer key '{nameIdentifier}'");
            }

            _groups[nameIdentifier] = value;
        }
    }

    public int Count => _groups.Count;
    public bool IsReadOnly => false;

    public void Add(GroupPolicy group)
    {
        group.Validate().ThrowOnError();
        _groups[group.NameIdentifier] = group;
    }

    public void AddUser(string nameIdentifier, string principalIdentifier, GraphTrxContext? trxContext = null)
    {
        if (_groups.TryGetValue(nameIdentifier, out var group))
        {
            var newGroup = group.Append(principalIdentifier);
            _groups[nameIdentifier] = newGroup;
            trxContext?.TransactionScope.GroupAdd(newGroup);
            return;
        }

        //group.AddUser(principalIdentifier, trxContext);
    }

    public void Clear() => _groups.Clear();
    public bool Contains(GroupPolicy item) => _groups.TryGetValue(item.NameIdentifier, out var existing) && existing == item;
    public bool Remove(GroupPolicy group) => _groups.TryRemove(group.NameIdentifier, out _);

    // Convenience helpers
    public bool Contains(string nameIdentifier) => _groups.ContainsKey(nameIdentifier);
    public bool TryGetGroup(string nameIdentifier, out GroupPolicy group) => _groups.TryGetValue(nameIdentifier, out group!);

    public bool InGroup(string groupIdentifier, string principalIdentifier)
    {
        if (_groups.TryGetValue(groupIdentifier, out var group)) return group.Members.Contains(principalIdentifier);
        return false;
    }

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

    public IEnumerator<GroupPolicy> GetEnumerator() => _groups.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _groups.Values.GetEnumerator();

    // Equality
    public override bool Equals(object? obj) => obj is GroupCollection other && Equals(other);

    // Order-independent hash code based on content; consistent with Equals
    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var pair in _groups)
        {
            // Combine key/value then xor to make order-independent
            hash ^= HashCode.Combine(pair.Key, pair.Value);
        }
        return hash;
    }

    public bool Equals(GroupCollection? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_groups.Count != other._groups.Count) return false;

        foreach (var item in _groups)
        {
            if (!other._groups.TryGetValue(item.Key, out var otherGroup)) return false;
            if (item.Value != otherGroup) return false;
        }

        return true;
    }

    public static bool operator ==(GroupCollection? left, GroupCollection? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(GroupCollection? left, GroupCollection? right) => !(left == right);
}
