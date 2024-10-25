using Toolbox.Tools;

namespace Toolbox.Graph;

internal class TagCollection
{
    public IDictionary<string, string?> Tags { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public void AddTag(string tag)
    {
        tag.NotEmpty();

        (string key, string? value) = tag.Split('=') switch
        {
            { Length: 1 } => (tag, null),
            { Length: 2 } v => (v[0], v[1]),
            _ => throw new ArgumentException("Invalid format"),
        };

        Tags[key] = value;
    }

    public void AddTag(string tag, string? value)
    {
        tag.NotEmpty();
        value.NotEmpty();

        Tags[tag] = value;
    }
}