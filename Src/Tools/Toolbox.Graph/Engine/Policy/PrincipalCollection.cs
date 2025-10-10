using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class PrincipalCollection : IEquatable<PrincipalCollection>, IEnumerable<PrincipalIdentity>
{
    private readonly ConcurrentDictionary<string, PrincipalIdentity> _principals;

    public PrincipalCollection() => _principals = new();

    public PrincipalCollection(IEnumerable<PrincipalIdentity> principals) => _principals = principals.NotNull()
        .ForEach(x => x.Validate().ThrowOnError())
        .ToConcurrentDictionary(x => x.NameIdentifier);

    // Add this property to expose the collection for JSON serialization
    public PrincipalIdentity this[string nameIdentifier]
    {
        get => _principals[nameIdentifier];
        set => Add(value);
    }

    public int Count => _principals.Count;

    // System.Text.Json requires: public void Add(T item) for custom collections
    public void Add(PrincipalIdentity principal)
    {
        principal.Validate().ThrowOnError();
        _principals[principal.NameIdentifier] = principal;
    }

    public bool Remove(string nameIdentifier) => _principals.Remove(nameIdentifier, out var _);

    public void Clear() => _principals.Clear();
    public bool Contains(string nameIdentifier) => _principals.ContainsKey(nameIdentifier);
    public bool Remove(PrincipalIdentity principalIdentity) => _principals.TryRemove(principalIdentity.PrincipalId, out _);
    public bool TryGetGroup(string principalId, out PrincipalIdentity principalIdentity) => _principals.TryGetValue(principalId, out principalIdentity!);

    // Fixed: Changed GroupCollection to PrincipalCollection
    public override bool Equals(object? obj) => obj is PrincipalCollection other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_principals);

    public bool Equals(PrincipalCollection? other) => other is not null &&
        _principals.Count == other._principals.Count &&
        _principals.All(pair =>
                    other._principals.TryGetValue(pair.Key, out var otherGroups) &&
                    pair.Value == otherGroups
                );

    public IEnumerator<PrincipalIdentity> GetEnumerator() => _principals.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(PrincipalCollection? left, PrincipalCollection? right) => left?.Equals(right) ?? false;
    public static bool operator !=(PrincipalCollection? left, PrincipalCollection? right) => !left?.Equals(right) ?? false;
}
