using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank;

public class SoftBankClient
{
    private readonly HttpClient _client;
    public SoftBankClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Create(ObjectId id, AccountDetail model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .ToOption();

    public Task<Option> Delete(ObjectId id, PrincipalId principalId, ScopeContext context) => _client.Delete(SpinConstants.Schema.SoftBank, id, principalId, context);

    public Task<Option> Exist(ObjectId id, PrincipalId principalId, ScopeContext context) => _client.Exist(SpinConstants.Schema.SoftBank, id, principalId, context);

    public async Task<Option<AccountDetail>> GetAccountDetail(ObjectId id, PrincipalId principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/accountDetail/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<AccountDetail>();

    public async Task<Option> SetAccountDetail(ObjectId id, AccountDetail accountDetail, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/accountDetail/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(accountDetail)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<AccountBalance>> GetBalance(ObjectId id, PrincipalId principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/balance{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<AccountBalance>();


    public async Task<Option> SetAcl(ObjectId id, BlockAcl blockAcl, PrincipalId principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/acl/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .SetContent(blockAcl)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> AddLedgerItem(ObjectId id, LedgerItem model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/ledgerItem/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(ObjectId id, PrincipalId principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.SoftBank}/ledgerItem/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<IReadOnlyList<LedgerItem>>();
}
