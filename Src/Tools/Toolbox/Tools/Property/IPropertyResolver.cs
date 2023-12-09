using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Tools
{
    public interface IPropertyResolver
    {
        bool HasProperty(string subject);

        [return: NotNullIfNotNull("subject")]
        string? Resolve(string? subject);
    }
}