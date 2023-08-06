using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Services;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

public interface IConfigurationActor : IGrainWithStringKey
{
    Task<Option<SiloConfigOption>> Get(string traceId);
    Task<Option> Set(SiloConfigOption model, string traceId);
}


public class ConfigurationActor : Grain, IConfigurationActor
{
    private readonly SiloConfigStore _configStore;
    private readonly IValidator<SiloConfigOption> _validator;
    private readonly ILogger _logger;
    private readonly SiloConfigurationAgent _siloConfigurationAgent;
    private readonly TenantListAgent _tenantListAgent;

    public ConfigurationActor(SiloConfigStore configStore, IValidator<SiloConfigOption> validator, ILogger<LeaseActor> logger)
    {
        _configStore = configStore;
        _validator = validator;
        _logger = logger;

        _siloConfigurationAgent = new SiloConfigurationAgent(configStore);
        _tenantListAgent = new TenantListAgent(() => GrainFactory.GetGrain<ISearchActor>(SpinConstants.SchemaSearch));
    }

    public virtual async Task<Option<SiloConfigOption>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        Option<SiloConfigOption> soloConfigOption = await _siloConfigurationAgent.Get(context);
        IReadOnlyList<string> tenantList = await _tenantListAgent.Get(context);

        var mergedConfig = soloConfigOption.Return(() => new SiloConfigOption()) switch
        {
            var v => v with { Tenants = v.Tenants.Concat(tenantList).ToArray() }
        };

        return mergedConfig;
    }

    public virtual async Task<Option> Set(SiloConfigOption model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Setting id={id}, model={model}", this.GetPrimaryKeyString(), model.ToJsonPascalSafe(new ScopeContext(_logger)));
        if (!_validator.Validate(model).LogResult(context.Location()).IsValid) return new Option(StatusCode.BadRequest);

        var statusResult = await _configStore.Set(model, context);
        return new Option(statusResult);
    }
}
