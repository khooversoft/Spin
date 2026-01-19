namespace Toolbox.Store;

public interface IStoreProvider
{
    string Name { get; }
}

public interface IStoreKeyProvider : IStoreProvider
{
    IKeyStore GetStore(SpaceDefinition definition);
    IKeyStore<T> GetStore<T>(SpaceDefinition definition, SpaceOption<T> options);
}

public interface IStoreListProvider : IStoreProvider
{
    IListStore<T> GetStore<T>(SpaceDefinition definition, SpaceOption<T> options);
}