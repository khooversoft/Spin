using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public record DirectoryDetail
{
    public DirectoryDetail(StorePathDetail pathDetail, DataETag data, LeaseRecord? leaseRecord = null)
    {
        PathDetail = pathDetail.NotNull();
        Data = data.NotNull();
        LeaseRecord = leaseRecord;
    }

    public StorePathDetail PathDetail { get; init; } = default!;
    public DataETag Data { get; init; }
    public LeaseRecord? LeaseRecord { get; init; }
}
