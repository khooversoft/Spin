using System.Diagnostics.CodeAnalysis;
using Toolbox.Azure.DataLake.Model;

namespace Toolbox.Azure.DataLake
{
    public interface IDataLakeStoreFactory
    {
        IDataLakeStore? CreateStore(string nameSpace);

        IDataLakeFileSystem? CreateFileSystem(string nameSpace);

        bool TryGetValue(string nameSpace, [MaybeNullWhen(false)] out DataLakeNamespace value);
    }
}