using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Azure.DataLake
{
    public interface IDatalakeFileSystem
    {
        Task Create(string name, CancellationToken token = default);

        Task CreateIfNotExist(string name, CancellationToken token = default);

        Task Delete(string name, CancellationToken token = default);

        Task DeleteIfExist(string name, CancellationToken token = default);

        Task<IReadOnlyList<string>> List(CancellationToken token = default);
    }
}