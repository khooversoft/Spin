using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Azure.DataLake
{
    public interface IDataLakeFileSystem
    {
        Task Create(string name, CancellationToken token);

        Task CreateIfNotExist(string name, CancellationToken token);

        Task Delete(string name, CancellationToken token);

        Task DeleteIfExist(string name, CancellationToken token);

        Task<IReadOnlyList<string>> List(CancellationToken token);
    }
}