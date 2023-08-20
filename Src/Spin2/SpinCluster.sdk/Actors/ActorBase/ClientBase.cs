using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public abstract class ClientBase<T>
{
    protected readonly HttpClient _client;
    protected private readonly string _rootPath;
    public ClientBase(HttpClient client, string rootPath) => (_client, _rootPath) = (client.NotNull(), rootPath.NotEmpty());

    public Task<Option> Delete(ObjectId id, ScopeContext context) => _client.Delete(_rootPath, id, null, context);
    public Task<Option> Exist(ObjectId id, ScopeContext context) => _client.Exist(_rootPath, id, null, context);
    public Task<Option<T>> Get(ObjectId id, ScopeContext context) => _client.Get<T>(_rootPath, id, null, context);
    public Task<Option> Set(ObjectId id, T content, ScopeContext context) => _client.Set<T>(_rootPath, id, content, null, context);
}
