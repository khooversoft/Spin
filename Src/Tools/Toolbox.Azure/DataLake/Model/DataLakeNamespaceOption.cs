using System.Collections.Generic;

namespace Toolbox.Azure.DataLake.Model
{
    public record DataLakeNamespaceOption
    {
        public IReadOnlyDictionary<string, DataLakeNamespace> Namespaces { get; init; } = null!;
    }
}