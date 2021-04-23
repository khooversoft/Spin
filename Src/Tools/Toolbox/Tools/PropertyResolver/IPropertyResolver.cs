using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Tools.PropertyResolver
{
    public interface IPropertyResolver
    {
        bool HasProperty(string subject);

        [return: NotNullIfNotNull("subject")]
        string? Resolve(string? subject);
    }
}