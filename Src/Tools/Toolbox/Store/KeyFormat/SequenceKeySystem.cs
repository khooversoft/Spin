using Toolbox.Tools;

namespace Toolbox.Store;

public class SequenceKeySystem<T> : KeySystemBase
{
    private readonly LogSequenceNumber _logSequenceNumber;

    public SequenceKeySystem(string basePath, LogSequenceNumber logSequenceNumber)
        : base(basePath, KeySystemType.Sequence)
    {
        _logSequenceNumber = logSequenceNumber;
    }

    public string PathBuilder(string key)
    {
        key.NotEmpty();
        DateTime now = DateTime.UtcNow;
        string seqNumber = _logSequenceNumber.Next();
        var result = $"{GetPathPrefix()}/{key}/{key}-{seqNumber}.{typeof(T).Name}.json";
        return result.ToLowerInvariant();
    }

    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractSequenceNumberIndex(path).LogTime;
}
