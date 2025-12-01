using Toolbox.Data;
using Toolbox.Types;

namespace Toolbox.Store;

public interface IStoreProvider
{
    string Name { get; }
}

public interface IStoreFileProvider : IStoreProvider
{
    IKeyStore GetStore(SpaceDefinition definition);
}

public interface IStoreListProvider : IStoreProvider
{
    IListStore<T> GetStore<T>(SpaceDefinition definition);
}