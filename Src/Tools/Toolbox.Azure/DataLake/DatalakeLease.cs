using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public readonly struct DatalakeLease
{
    public DatalakeLease(string path, DataETag data, string leaseId)
    {
        Path = path.NotNull();
        Data = data;
        LeaseId = leaseId.NotEmpty();
    }

    public string Path { get; }
    public DataETag Data { get; }
    public string LeaseId { get; }

    public DatalakeLease WithData(DataETag data) => new DatalakeLease(Path, data, LeaseId);
}
