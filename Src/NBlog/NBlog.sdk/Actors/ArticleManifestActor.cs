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
    private readonly TimeSpan _cacheTime = TimeSpan.FromMinutes(15);
    private readonly IPersistentState<ArticleManifest> _state;
    private readonly ILogger<ArticleManifestActor> _logger;
    private DateTime _nextRead;

    public ArticleManifestActor([PersistentState(stateName: "default")] IPersistentState<ArticleManifest> state, ILogger<ArticleManifestActor> logger)
    {
        _state = state.NotNull();
        _logger = logger.NotNull();

        ResetNextRead();
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

        if (!_state.RecordExists) return StatusCode.NotFound;
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public Task<Option> Exist(string traceId) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option<ArticleManifest>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (DateTime.UtcNow > _nextRead && _state.RecordExists)
        {
            ResetNextRead();
            await _state.ReadStateAsync();
        }

        var option = _state.RecordExists switch
        {
            true => _state.State,
            false => new Option<ArticleManifest>(StatusCode.NotFound),
        };

        return option;
    }

    public async Task<Option> Set(ArticleManifest model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set ArticleManifest, actorKey={actorKey}", this.GetPrimaryKeyString());

        string actorKey = this.GetPrimaryKeyString();
        if (actorKey.EqualsIgnoreCase(model.ArticleId)) return (StatusCode.BadRequest, $"ArticleId={model.ArticleId} does not match actorKey={actorKey}");
        if (!model.Validate(out var v1)) return v1;

        _state.State = model;
        await _state.WriteStateAsync();

        ResetNextRead();
        return StatusCode.OK;
    }

    private void ResetNextRead() => _nextRead = DateTime.UtcNow.Add(_cacheTime);
}
