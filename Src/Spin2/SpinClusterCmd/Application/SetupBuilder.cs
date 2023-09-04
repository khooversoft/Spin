using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
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

        await CreateAllSubscriptions(services, context);
        await CreateAllTenants(services, context);
        await CreateAllUsers(services, context);
        await CreateAllAccounts(services, context);
    }

    private async Task DeleteAllAccounts(IServiceProvider services, ScopeContext context)
    {
        SoftBankClient client = services.GetRequiredService<SoftBankClient>();

        foreach (var item in _option.Accounts)
        {
            await client.Delete(item.AccountId, context);
            context.Location().LogInformation("Account deleted: {accountId}", item.AccountId);
        }
    }

    private async Task DeleteAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        foreach (var user in _option.Users)
        {
            await client.Delete(user.UserId, context);
            context.Location().LogInformation("User deleted: {user}", user.UserId);
        }
    }

    private async Task DeleteAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        foreach (var item in _option.Tenants)
        {
            await client.Delete(item.Domain, context);
            context.Location().LogInformation("Tenant deleted: {domains}", item.Domain);
        }
    }

    private async Task DeleteAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        foreach (var item in _option.Subscriptions)
        {
            await client.Delete(item.Name, context);
           context.Location().LogInformation("Subscription deleted: {name}", item.Name);
        }
    }

    private async Task<Option> CreateAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        var test = new Option();

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

            context.Location().LogStatus(setOption, "Creating Subscription name={name}", item.Name);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        var test = new Option();

        foreach (var item in _option.Tenants)
        {
            Option<TenantModel> result = await client.Get(item.Domain, context);
            if (result.IsOk()) await client.Delete(item.Domain, context);

            var tenant = new TenantModel
            {
                TenantId = IdTool.CreateTenantId(item.Domain),
                Name = item.Domain,
                SubscriptionId = IdTool.CreateSubscriptionId(item.Subscription),
                ContactName = item.ContactName,
                Email = item.Email,

                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(tenant, context);

            context.Location().LogStatus(setOption, "Creating Tenant domain={domain}", item.Domain);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        var test = new Option();

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

            context.Location().LogStatus(setOption, "Creating User userId={userId}", item.UserId);
            test.Test(() => setOption);
        }

        return test;
    }

    private async Task<Option> CreateAllAccounts(IServiceProvider services, ScopeContext context)
    {
        SoftBankClient softBankClient = services.GetRequiredService<SoftBankClient>();

        var test = new Option();

        foreach (var item in _option.Accounts)
        {
            var existOption = await softBankClient.Exist(item.AccountId, context);
            if (existOption.IsOk()) await softBankClient.Delete(item.AccountId, context);

            var createRequest = new AccountDetail
            {
                DocumentId = item.AccountId,
                OwnerId = item.PrincipalId,
                Name = item.Name,
                AccessRights = (item.WriteAccess ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new AccessBlock { BlockType = nameof(LedgerItem), PrincipalId = x, Grant = BlockGrant.ReadWrite })
                    .ToArray(),
            };

            var createOption = await softBankClient.Create(createRequest, context);

            context.Location().LogStatus(createOption, "Creating Account accountId={accountId}", item.AccountId);
            test.Test(() => createOption);
        }

        return test;
    }
}
