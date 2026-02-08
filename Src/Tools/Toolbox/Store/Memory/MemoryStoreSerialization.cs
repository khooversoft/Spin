using System.Text.Json.Serialization;

namespace Toolbox.Store;

public class MemoryStoreSerialization
{
    public MemoryStoreSerialization(IEnumerable<DirectoryDetail> directoryDetails, string? logSequenceNumber)
    {
        DirectoryDetails = directoryDetails.ToArray();
        LogSequenceNumber = logSequenceNumber;
    }

    [JsonConstructor]
    public MemoryStoreSerialization(IReadOnlyList<DirectoryDetail> directoryDetails, string? logSequenceNumber)
    {
        DirectoryDetails = directoryDetails.ToArray();
        LogSequenceNumber = logSequenceNumber;
    }

    public IReadOnlyList<DirectoryDetail> DirectoryDetails { get; } = Array.Empty<DirectoryDetail>();
    public string? LogSequenceNumber { get; }
}
