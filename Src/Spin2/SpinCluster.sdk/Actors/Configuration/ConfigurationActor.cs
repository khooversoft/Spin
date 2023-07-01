using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface IConfigurationActor : IGrainWithStringKey
{
    Task<SpinResponse<SiloConfigOption>> Get(string traceId);
    Task<StatusCode> Set(SiloConfigOption model, string traceId);
}


public class ConfigurationActor : Grain, IConfigurationActor
{
    private readonly SiloConfigStore _configStore;
    private readonly Validator<SiloConfigOption> _validator;
    private readonly CacheObject<SiloConfigOption> _cacheObject = new CacheObject<SiloConfigOption>(TimeSpan.FromMinutes(15));
    private readonly ILogger _logger;

    public ConfigurationActor(SiloConfigStore configStore, Validator<SiloConfigOption> validator, ILogger<LeaseActor> logger)
    {
        _configStore = configStore;
        _validator = validator;
        _logger = logger;
    }

    public virtual async Task<SpinResponse<SiloConfigOption>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting SpinConfigruation id={id}", this.GetPrimaryKeyString());

        if (_cacheObject.TryGetValue(out SiloConfigOption value)) return value;

        SiloConfigOption option = await _configStore.Get(context).Return();
        _cacheObject.Set(option);

        return option;
    }

    public virtual async Task<StatusCode> Set(SiloConfigOption model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Setting id={id}, model={model}", this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));
        if (!_validator.Validate(model).IsValid(context.Location())) return StatusCode.BadRequest;

        return await _configStore.Set(model, context);
    }
}
