using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IArticleManifestActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<ArticleManifest>> Get(string traceId);
    Task<Option> Set(ArticleManifest model, string traceId);
}

public class ArticleManifestActor : Grain, IArticleManifestActor
{
    private readonly ILogger<ArticleManifestActor> _logger;
    private ActorCacheState<ArticleManifest> _state;

    public ArticleManifestActor([PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<ArticleManifest> state, ILogger<ArticleManifestActor> logger)
    {
        _logger = logger.NotNull();
        _state = new ActorCacheState<ArticleManifest>(state, TimeSpan.FromMinutes(15));
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        string actorKey = this.GetPrimaryKeyString();
        FileId.Create(actorKey).ThrowOnError("Actor Id is invalid");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        return await _state.Clear();
    }

    public async Task<Option> Exist(string traceId) => (await _state.GetState()).ToOptionStatus();

    public async Task<Option<ArticleManifest>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        var state = await _state.GetState();
        return state;
    }

    public async Task<Option> Set(ArticleManifest model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        string actorKey = this.GetPrimaryKeyString();
        if (actorKey.EqualsIgnoreCase(model.ArticleId)) return (StatusCode.BadRequest, $"ArticleId={model.ArticleId} does not match actorKey={actorKey}");
        if (!model.Validate(out var v1)) return v1;

        return await _state.SetState(model);
    }
}
