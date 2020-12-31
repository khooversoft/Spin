using Toolbox.Tools;

namespace Toolbox.Azure.DataLake.Model
{
    public record DataLakeStoreOption
    {
        public string AccountName { get; init; } = null!;

        public string AccountKey { get; init; } = null!;

        public string ContainerName { get; init; } = null!;
    }
}