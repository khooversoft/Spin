namespace Toolbox.Store;

public interface IStoreProvider
{
    string Name { get; }
}

public interface IStoreKeyProvider : IStoreProvider
{
    IKeyStore GetStore(SpaceDefinition definition);
}

public interface IStoreListProvider : IStoreProvider
{
    IListStore2<T> GetStore<T>(SpaceDefinition definition);
}