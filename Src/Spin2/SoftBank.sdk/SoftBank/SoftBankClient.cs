﻿using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

public class SoftBankClient
{
    private readonly HttpClient _client;
    private readonly ILogger<SoftBankClient> _logger;

    public SoftBankClient(HttpClient client, ILogger<SoftBankClient> logger)
    {
        _client = client.NotNull();
        _logger = logger;
    }

    public Task<Option> Delete(string accountId, ScopeContext context) => new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Exist(string accountId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/exist")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> Create(AccountDetail content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> SetAccountDetail(AccountDetail content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/accountdetail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> SetAcl(string accountId, AclBlock content, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/acl")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> AddLedgerItem(string accountId, LedgerItem content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/ledgerItem")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<AccountDetail>> GetAccountDetail(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/accountDetail")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context.With(_logger))
        .GetContent<AccountDetail>();

    public async Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/ledgerItem")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context.With(_logger))
        .GetContent<IReadOnlyList<LedgerItem>>();

    public async Task<Option<AccountBalance>> GetBalance(string accountId, string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{IdSoftbank.SoftBankSchema}/{Uri.EscapeDataString(accountId)}/balance")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId)
        .GetAsync(context.With(_logger))
        .GetContent<AccountBalance>();
}
