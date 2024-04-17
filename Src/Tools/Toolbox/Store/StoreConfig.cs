using Toolbox.Tools;

namespace Toolbox.Store;

public sealed record StoreConfig
{
    public StoreConfig(string alias, Func<IServiceProvider, StoreConfig, IFileStore> create)
    {
        Alias = alias.NotEmpty();
        Create = create.NotNull();
    }

    public string Alias { get; }
    public Func<IServiceProvider, StoreConfig, IFileStore> Create { get; }
}
