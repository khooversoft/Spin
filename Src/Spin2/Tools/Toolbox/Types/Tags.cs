using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public class Tags : Dictionary<string, string?>
{
    public Tags Set(string? subject)
    {
        if (subject.IsEmpty()) return this;

        var keyValuePairs = subject.ToDictionary();
        keyValuePairs.ForEach(x => this[x.Key] = x.Value);
        return this;
    }

    public Tags Set(string key, string? value) => this.Action(x => this[key.NotEmpty()] = value);

    public override string ToString() => this.Select(x => x.Value.IsEmpty() ? x.Key : $"{x.Key}={x.Value}").Join(';');
    public string ToString(bool order) => order switch
    {
        false => this.ToString(),
        true => this.OrderBy(x => x.Key).Select(x => x.Value.IsEmpty() ? x.Key : $"{x.Key}={x.Value}").Join(';'),
    };

    public override bool Equals(object? obj) => Equals(obj as Tags);
    public bool Equals(Tags? other) => other is not null &&
        this.Count == other.Count &&
        this.All(x => other.TryGetValue(x.Key, out var subject) && x.Value == subject);

    public override int GetHashCode() => HashCode.Combine(this);
    public static bool operator ==(Tags? left, Tags? right) => EqualityComparer<Tags>.Default.Equals(left, right);
    public static bool operator !=(Tags? left, Tags? right) => !(left == right);

    public static Tags Parse(string? subject) => new Tags().Set(subject);
}
