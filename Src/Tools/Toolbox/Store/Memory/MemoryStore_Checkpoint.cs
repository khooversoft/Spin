using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Store;

public partial class MemoryStore : ICheckpoint
{
    public Task<string> Checkpoint()
    {
        lock (_lock)
        {
            DirectoryDetail[] list = _store.Values.ToArray();
            var json = list.ToJson();
            return json.ToTaskResult();
        }
    }

    public Task<Option> Recovery(string json)
    {
        _logger.LogWarning("Recovering store from json.");

        lock (_lock)
        {
            _store.Clear();
            _leaseStore.Clear();

            DirectoryDetail[] list = json.NotEmpty().ToObject<DirectoryDetail[]>().NotNull();

            foreach (var item in list)
            {
                _store[item.PathDetail.Path] = item;
            }

            return new Option(StatusCode.OK).ToTaskResult();
        }
    }
}
