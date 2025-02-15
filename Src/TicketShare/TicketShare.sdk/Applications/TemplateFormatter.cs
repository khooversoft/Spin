using System.Collections.Frozen;
using Toolbox.Email;
using Toolbox.Tools;

namespace TicketShare.sdk;

public readonly struct TemplateFormatter
{
    public TemplateFormatter(string templateFile, IEnumerable<KeyValuePair<string, string>> properties)
    {
        TemplateFile = templateFile.NotEmpty();
        Properties = properties.NotNull().ToFrozenDictionary(x => x.Key, x => x.Value);
    }

    public string TemplateFile { get; init; }
    public IReadOnlyDictionary<string, string> Properties { get; init; } = FrozenDictionary<string, string>.Empty;

    public string Build()
    {
        string html = AssemblyResource.GetResourceString($"TicketShare.sdk.Applications.Email.Templates.{TemplateFile}", typeof(TemplateFormatter));
        html = Properties.Aggregate(html, (current, next) => current.Replace($"{{{next.Key}}}", next.Value));
        return html;
    }
}