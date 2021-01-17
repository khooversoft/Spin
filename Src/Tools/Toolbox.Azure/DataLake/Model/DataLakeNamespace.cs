using Toolbox.Tools;

namespace Toolbox.Azure.DataLake.Model
{
    public record DataLakeNamespace
    {
        public string Namespace { get; init; } = null!;

        public string? PathRoot { get; init; }

        public DataLakeStoreOption Store { get; init; } = null!;
    }
}