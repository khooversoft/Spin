using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Tools.Property
{
    public interface IPropertyResolver
    {
        bool HasProperty(string subject);

        [return: NotNullIfNotNull("subject")]
        string? Resolve(string? subject);
    }
}