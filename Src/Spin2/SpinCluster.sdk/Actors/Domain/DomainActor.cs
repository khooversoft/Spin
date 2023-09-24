using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Domain;

public interface IDomainActor : IGrainWithStringKey
{
    Task<Option<DomainDetail>> GetDetails(string domain, string traceId);
    Task<DomainList> List(string traceId);
    Task<Option> SetExternalDomain(string domain, string traceId);
    Task<Option> RemoveExternalDomain(string domain, string traceId);
}

[StatelessWorker]
[Reentrant]
public class DomainActor : Grain, IDomainActor
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<DomainActor> _logger;

    public DomainActor(IClusterClient clusterClient, ILogger<DomainActor> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.GetPrimaryKeyString()
            .Assert(x => x == SpinConstants.DomainActorKey, x => $"Actor key {x} is invalid, must = {SpinConstants.DomainActorKey}");

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option<DomainDetail>> GetDetails(string domain, string traceId)
    {
        if (!IdPatterns.IsDomain(domain)) return new Option<DomainDetail>(StatusCode.BadRequest, "Invalid domain");

        var response = await isExternal(domain) switch
        {
            true => new DomainDetail { Domain = domain },
            false => await lookupTenant(domain) switch
            {
                var v when v.IsError() => v.ToOptionStatus<DomainDetail>(),
                var v => new DomainDetail { Domain = domain, TenantId = v.Return().TenantId }
            }
        };

        return response;


        async Task<bool> isExternal(string domain)
        {
            DomainList list = await List(traceId);

            var result = list
                .ExternalDomains
                .FirstOrDefaultOption(x => x == domain);

            if (result.IsNoContent()) return false;
            return true;
        }

        async Task<Option<TenantModel>> lookupTenant(string domain)
        {
            string id = $"tenant:{domain}";

            return await _clusterClient
                .GetResourceGrain<ITenantActor>(id)
                .Get(traceId);
        }
    }

    public async Task<DomainList> List(string traceId)
    {
        var context = new ScopeContext(_logger);

        var configModel = await GetConfig(context);
        if (configModel.IsError()) return new DomainList();

        var result = new DomainList
        {
            ExternalDomains = configModel.Return()
                .Properties
                .Select(x => x.Key)
                .ToArray(),
        };

        return result;
    }

    public async Task<Option> RemoveExternalDomain(string domain, string traceId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Removing external domain, domain={domain}", domain);

        var remove = new RemovePropertyModel
        {
            ConfigId = SpinConstants.ConfigKeys.ValidDomainActorKey,
            Key = domain,
        };

        var result = await _clusterClient
            .GetResourceGrain<IConfigActor>(SpinConstants.ConfigKeys.ValidDomainActorKey)
            .RemoveProperty(remove, context.TraceId);

        return result;
    }

    public async Task<Option> SetExternalDomain(string domain, string traceId)
    {
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Adding external domain, domain={domain}", domain);

        var set = new SetPropertyModel
        {
            ConfigId = SpinConstants.ConfigKeys.ValidDomainActorKey,
            Key = domain,
            Value = "true",
        };

        var result = await _clusterClient
            .GetResourceGrain<IConfigActor>(SpinConstants.ConfigKeys.ValidDomainActorKey)
            .SetProperty(set, context.TraceId);

        return result;
    }

    private async Task<Option<ConfigModel>> GetConfig(ScopeContext context)
    {
        var configModel = await _clusterClient
            .GetResourceGrain<IConfigActor>(SpinConstants.ConfigKeys.ValidDomainActorKey)
            .Get(context.TraceId);

        if (configModel.IsError())
        {
            context.Location().LogError("Configuration actorKey={actorKey} does not exist",
                SpinConstants.ConfigKeys.ValidDomainActorKey);

            return configModel;
        }

        return configModel;
    }
}
