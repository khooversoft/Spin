using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Data;
using Toolbox.DocumentSearch;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface ISearchActor : IGrainWithStringKey
{
    Task<Option<IReadOnlyList<DocumentReference>>> Search(string query, string traceId);
}

public class SearchActor : Grain, ISearchActor
{
    private readonly ILogger<SearchActor> _logger;
    private readonly ActorCacheState<DocumentIndex, DocumentIndexSerialization> _state;

    public SearchActor(
        StateManagement stateManagement,
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<DocumentIndexSerialization> state,
        ILogger<SearchActor> logger
        )
    {
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<DocumentIndex, DocumentIndexSerialization>(
            stateManagement,
            state, x => x.ToSerialization(),
            x => x.FromSerialization(),
            TimeSpan.FromMinutes(15)
            );
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(NBlogConstants.Tool.IsSearchActorKey, x => $"ActorKey={x} is not valid.");

        _state.SetName(nameof(SearchActor), this.GetPrimaryKeyString());
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<IReadOnlyList<DocumentReference>>> Search(string query, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        Option<DocumentIndex> indexOption = await _state.GetState(context);
        if (indexOption.IsError())
        {
            context.Location().LogError("Cannot get document index from storage");
            return indexOption.ToOptionStatus<IReadOnlyList<DocumentReference>>();
        }

        DocumentIndex index = indexOption.Return();
        IReadOnlyList<DocumentReference> result = index.Search(query);
        return result.ToOption();
    }
}
