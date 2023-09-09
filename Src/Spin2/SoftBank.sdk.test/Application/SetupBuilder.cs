using FluentAssertions;
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

namespace SoftBank.sdk.test.Application;

internal record TenantInfo(string Subscription, string Tenant);
internal record AccountInfo(string AccountId, string PrincipalId, string[] WriteAccess);

internal class SetupBuilder
{
    public List<string> Subscriptions { get; init; } = new List<string>();
    public List<TenantInfo> Tenants { get; init; } = new List<TenantInfo>();
    public List<string> Users { get; init; } = new List<string>();
    public List<AccountInfo> Accounts { get; init; } = new List<AccountInfo>();

    public SetupBuilder AddSubscription(string subscription) => this.Action(_ => Subscriptions.Add(subscription.NotEmpty()));
    public SetupBuilder AddTenant(string subscription, string tenant) => this.Action(_ => Tenants.Add(new TenantInfo(subscription.NotEmpty(), tenant.NotEmpty())));
    public SetupBuilder AddUser(string principalId) => this.Action(_ => Users.Add(principalId.NotEmpty()));
    public SetupBuilder AddAccount(string accountId, string principalId, params string[] writeAccess) =>
        this.Action(_ => Accounts.Add(new AccountInfo(accountId.NotEmpty(), principalId.NotEmpty(), writeAccess)));

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

        foreach (var item in Accounts)
        {
            await client.Delete(item.AccountId, context);
        }
    }

    private async Task DeleteAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        foreach (var principalId in Users)
        {
            await client.Delete(principalId, context);
        }
    }

    private async Task DeleteAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        foreach (var item in Tenants)
        {
            await client.Delete(item.Tenant, context);
        }
    }

    private async Task DeleteAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        foreach (var item in Subscriptions)
        {
            await client.Delete(item, context);
        }
    }

    private async Task CreateAllSubscriptions(IServiceProvider services, ScopeContext context)
    {
        SubscriptionClient client = services.GetRequiredService<SubscriptionClient>();

        foreach (var nameId in Subscriptions)
        {
            Option<SubscriptionModel> result = await client.Get(nameId, context);
            if (result.IsOk()) await client.Delete(nameId, context);

            var subscription = new SubscriptionModel
            {
                SubscriptionId = IdTool.CreateSubscriptionId(nameId),
                Name = nameId,
                ContactName = nameId + "contact",
                Email = "user1@company1.com",
                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(subscription, context);
            setOption.StatusCode.IsOk().Should().BeTrue();
        }
    }

    private async Task CreateAllTenants(IServiceProvider services, ScopeContext context)
    {
        TenantClient client = services.GetRequiredService<TenantClient>();

        foreach (var item in Tenants)
        {
            Option<TenantModel> result = await client.Get(item.Tenant, context);
            if (result.IsOk()) await client.Delete(item.Tenant, context);

            var tenant = new TenantModel
            {
                TenantId = IdTool.CreateTenantId(item.Tenant),
                Domain = item.Tenant,
                SubscriptionId = IdTool.CreateSubscriptionId(item.Subscription),
                ContactName = item.Tenant + "contact",
                Email = "user1@company2.com",

                AccountEnabled = true,
                ActiveDate = DateTime.UtcNow,
            };

            Option setOption = await client.Set(tenant, context);
            setOption.StatusCode.IsOk().Should().BeTrue();
        }
    }

    private async Task CreateAllUsers(IServiceProvider services, ScopeContext context)
    {
        UserClient client = services.GetRequiredService<UserClient>();

        foreach (var principalId in Users)
        {
            Option<UserModel> result = await client.Get(principalId, context);
            if (result.IsOk()) await client.Delete(principalId, context);

            var user = new UserCreateModel
            {
                UserId = IdTool.CreateUserId(principalId),
                PrincipalId = principalId,
                DisplayName = "User display name",
                FirstName = "First",
                LastName = "Last"
            };

            Option setOption = await client.Create(user, context);
            setOption.IsOk().Should().BeTrue();
        }
    }

    private async Task CreateAllAccounts(IServiceProvider services, ScopeContext context)
    {
        SoftBankClient softBankClient = services.GetRequiredService<SoftBankClient>();

        foreach (var item in Accounts)
        {
            var existOption = await softBankClient.Exist(item.AccountId, context);
            if (existOption.IsOk()) await softBankClient.Delete(item.AccountId, context);

            var createRequest = new AccountDetail
            {
                AccountId = item.AccountId,
                OwnerId = item.PrincipalId,
                Name = "test account",
                AccessRights = item.WriteAccess
                    .Select(x => new AccessBlock { BlockType = nameof(LedgerItem), PrincipalId = x, Grant = BlockGrant.ReadWrite })
                    .ToArray(),
            };

            var createOption = await softBankClient.Create(createRequest, context);
            createOption.IsOk().Should().BeTrue();

            var readAccountDetailOption = await softBankClient.GetAccountDetail(item.AccountId, item.PrincipalId, context);
            readAccountDetailOption.IsOk().Should().BeTrue();

            var readAccountDetail = readAccountDetailOption.Return();
            (createRequest = readAccountDetail).Should().NotBeNull();
        }
    }
}
