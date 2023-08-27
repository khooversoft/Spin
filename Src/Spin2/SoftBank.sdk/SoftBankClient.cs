using SoftBank.sdk.Models;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk;

public class SoftBankClient
{
    private const string _schema = "softbank";

    private readonly HttpClient _client;
    public SoftBankClient(HttpClient client) => _client = client.NotNull();

    public Task<Option> Delete(string accountId, ScopeContext context) => new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option> Exist(string accountId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public async Task<Option> Create(AccountDetail content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> SetAccountDetail(AccountDetail content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/accountdetail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> SetAcl(string accountId, BlockAcl content, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/acl")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> AddLedgerItem(string accountId, LedgerItem content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/ledgerItem")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option<AccountDetail>> GetAccountDetail(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/accountDetail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<AccountDetail>();

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/ledgerItem")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<IReadOnlyList<LedgerItem>>();

    public async Task<Option<AccountBalance>> GetBalance(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_schema}/{Uri.EscapeDataString(accountId)}/balance")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context)
        .GetContent<AccountBalance>();
}
