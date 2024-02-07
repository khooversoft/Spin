using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public interface IProfanityFilterActor : IGrainWithStringKey
{
    Task<Option<string>> Filter(string filter, string traceId);
}

public class ProfanityFilterActor : Grain, IProfanityFilterActor
{
    private readonly ActorCacheState<ProfanityConfiguration> _state;
    private readonly ILogger<ProfanityFilterActor> _logger;
    private FrozenSet<string> _badWords = null!;

    public ProfanityFilterActor(
        StateManagement stateManagement,
        [PersistentState("default", NBlogConstants.DataLakeProviderName)] IPersistentState<ProfanityConfiguration> state,
        ILogger<ProfanityFilterActor> logger
        )
    {
        _logger = logger.NotNull();
        stateManagement.NotNull();

        _state = new ActorCacheState<ProfanityConfiguration>(stateManagement, state, TimeSpan.FromMinutes(15));
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString().Assert(x => x == NBlogConstants.BadWordsActorKey, x => $"ActorKey={x} is not valid.");

        _state.SetName(nameof(ProfanityFilterActor), this.GetPrimaryKeyString());
        if (_state.RecordExists)
        {
            _badWords = _state.State.Words.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }

        return base.OnActivateAsync(cancellationToken);
    }

    public Task<Option<string>> Filter(string source, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        return process().ToTaskResult();

        Option<string> process()
        {
            if (!_state.RecordExists || _badWords == null) return (StatusCode.NotFound, "Profanity data not available");
            if (source.IsEmpty()) return source;

            foreach (var badWord in _badWords)
            {
                source = source.Replace(badWord, string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            return source;
        }
    }
}