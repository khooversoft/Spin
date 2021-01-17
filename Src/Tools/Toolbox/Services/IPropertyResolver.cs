using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Services
{
    public interface IPropertyResolver
    {
        [return: NotNullIfNotNull("subject")]
        string? Resolve(string? subject);
    }
}