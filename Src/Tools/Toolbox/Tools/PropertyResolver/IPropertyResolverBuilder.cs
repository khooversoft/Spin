using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Toolbox.Tools.PropertyResolver
{
    public interface IPropertyResolverBuilder
    {
        string this[string key] { get; set; }

        void Build(string file);
        void BuildForSecretId(string secretId);
        IReadOnlyList<KeyValuePair<string, string>> List();
        bool Remove(string key);
        void Set(string key, string value);
        bool TryGetValue(string key, [NotNullWhen(true)] out string? value);
    }
}