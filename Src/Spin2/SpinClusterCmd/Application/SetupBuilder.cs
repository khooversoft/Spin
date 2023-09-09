using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterCmd.Application;

internal class SetupBuilder
{
    private readonly ScenarioOption _option;
    public SetupBuilder(ScenarioOption option) => _option = option;

    public async Task Build(IServiceProvider services, ScopeContext context)
    {
        await DeleteAllAccounts(services, context);
        await DeleteAllUsers(services, context);
        await DeleteAllTenants(services, context);
        await DeleteAllSubscriptions(services, context);
        await DeleteAgents(services, context);
        await DeleteSmartcItems(services, context);

        await CreateAllSubscriptions(services, context);
        await CreateAllTenants(services, context);
        await CreateAllUsers(services, context);
        await CreateAllAccounts(services, context);
        await CreateAgent(services, context);
        await CreateSmartc(services, context);
    }

    private async Task DeleteAllAccounts(IServiceProvider services, ScopeContext context)
    {
        SoftBankClient client = services.GetRequiredService<SoftBankClient>();

        foreach (var item in _option.Accounts)
        {
            await client.Delete(item.AccountId, context);
            context.Trace().LogInformation("Account deleted: {accountId}", item.AccountId);
        }
    }

    private async Task DeleteAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        foreach (var user in _option.Users)
        {
            await client.Delete(user.UserId, context);
            context.Trace().LogInformation("User deleted: {user}", user.UserId);
        }
    }

    private async Task DeleteAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        foreach (var item in _option.Tenants)
        {
            await client.Delete(item.Domain, context);
            context.Trace().LogInformation("Tenant deleted: {domains}", item.Domain);
        }
    }

    private async Task DeleteAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        foreach (var item in _option.Subscriptions)
        {
            await client.Delete(item.Name, context);
            context.Trace().LogInformation("Subscription deleted: {name}", item.Name);
        }
    }

    private async Task DeleteAgents(IServiceProvider services, ScopeContext context)
    {
        AgentClient client = services.GetRequiredService<AgentClient>();

        foreach (var item in _option.Agents)
        {
            await client.Delete(item.AgentId, context);
            context.Trace().LogInformation("Agent deleted: {agentId}", item.AgentId);
        }
    }

    private async Task DeleteSmartcItems(IServiceProvider services, ScopeContext context)
    {
        SmartcClient client = services.GetRequiredService<SmartcClient>();

        foreach (var item in _option.SmartcItems)
        {
            await client.Delete(item.SmartcId, context);
            context.Trace().LogInformation("SmartC deleted: {smartcId}", item.SmartcId);
        }
    }

    private async Task<Option> CreateAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        var test = new OptionTest();

        foreach (var item in _option.Subscriptions)
        {
            Option<SubscriptionModel> result = await client.Get(item.Name, context);
            if (result.IsOk()) await client.Delete(item.Name, context);

            var subscription = new SubscriptionModel
            {
                SubscriptionId = IdTool.CreateSubscriptionId(item.Name),
                Name = item.Name,
                ContactName = item.ContactName,
                Email = item.Email,
                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(subscription, context);

            context.Trace().LogStatus(setOption, "Creating Subscription name={name}", item.Name);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        var test = new OptionTest();

        foreach (var item in _option.Tenants)
        {
            Option<TenantModel> result = await client.Get(item.Domain, context);
            if (result.IsOk()) await client.Delete(item.Domain, context);

            var tenant = new TenantModel
            {
                TenantId = IdTool.CreateTenantId(item.Domain),
                Domain = item.Domain,
                SubscriptionId = IdTool.CreateSubscriptionId(item.Subscription),
                ContactName = item.ContactName,
                Email = item.Email,

                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(tenant, context);

            context.Trace().LogStatus(setOption, "Creating Tenant domain={domain}", item.Domain);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        var test = new OptionTest();

        foreach (var item in _option.Users)
        {
            Option<UserModel> result = await client.Get(item.UserId, context);
            if (result.IsOk()) await client.Delete(item.UserId, context);

            var user = new UserCreateModel
            {
                UserId = IdTool.CreateUserId(item.UserId),
                PrincipalId = item.UserId,
                DisplayName = item.DisplayName,
                FirstName = item.FirstName,
                LastName = item.LastName,
            };

            Option setOption = await client.Create(user, context);

            context.Trace().LogStatus(setOption, "Creating User userId={userId}", item.UserId);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllAccounts(IServiceProvider services, ScopeContext context)
    {
        SoftBankClient softBankClient = services.GetRequiredService<SoftBankClient>();

        var test = new OptionTest();

        foreach (var account in _option.Accounts)
        {
            var existOption = await softBankClient.Exist(account.AccountId, context);
            if (existOption.IsOk()) await softBankClient.Delete(account.AccountId, context);

            var createRequest = new AccountDetail
            {
                AccountId = account.AccountId,
                OwnerId = account.PrincipalId,
                Name = account.Name,
                AccessRights = (account.WriteAccess ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new AccessBlock { BlockType = nameof(LedgerItem), PrincipalId = x, Grant = BlockGrant.ReadWrite })
                    .ToArray(),
            };

            var createOption = await softBankClient.Create(createRequest, context);
            context.Trace().LogStatus(createOption, "Creating Account accountId={accountId}", account.AccountId);
            test.Test(() => createOption);

            foreach (var ledgerItem in account.LedgerItems)
            {
                var ledger = new LedgerItem
                {
                    OwnerId = ledgerItem.OwnerId,
                    Description = "Ledger-" + Guid.NewGuid().ToString(),
                    Type = ledgerItem.Amount > 0 ? LedgerType.Credit : LedgerType.Debit,
                    Amount = Math.Abs(ledgerItem.Amount),
                };

                var addResponse = await softBankClient.AddLedgerItem(account.AccountId, ledger, context);
                context.Trace().LogStatus(addResponse, "Add ledger item accountId={accountId}, amount={amount}", account.AccountId, ledger.Amount);
                test.Test(() => addResponse);
            }
        }

        return test;
    }

    private async Task<Option> CreateAgent(IServiceProvider services, ScopeContext context)
    {
        AgentClient client = services.GetRequiredService<AgentClient>();

        var test = new OptionTest();

        foreach (var item in _option.Agents)
        {
            Option<AgentModel> result = await client.Get(item.AgentId, context);
            if (result.IsOk()) await client.Delete(item.AgentId, context);

            var model = new AgentModel
            {
                AgentId = item.AgentId,
                Enabled = true
            };

            Option setOption = await client.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating Agent agentId={agentId}", item.AgentId);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateSmartc(IServiceProvider services, ScopeContext context)
    {
        SmartcClient client = services.GetRequiredService<SmartcClient>();

        var test = new OptionTest();

        foreach (var item in _option.SmartcItems)
        {
            Option<SmartcModel> result = await client.Get(item.SmartcId, context);
            if (result.IsOk()) await client.Delete(item.SmartcId, context);

            var model = new SmartcModel
            {
                SmartcId = item.SmartcId,
                Enabled = true
            };

            Option setOption = await client.Set(model, context);

            context.Trace().LogStatus(setOption, "Creating Tenant smartcId={smartcId}", item.SmartcId);
            test.Test(() => setOption);
        }

        return test;
    }
}
