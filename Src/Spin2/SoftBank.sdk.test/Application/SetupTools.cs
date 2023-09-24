using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public class SetupTools
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context;

    public SetupTools(ClusterApiFixture cluster, ScopeContext context)
    {
        _cluster = cluster;
        _context = context;
    }

    public async Task<Option<SubscriptionModel>> CreateSubscription(string nameId, ScopeContext context)
    {
        SubscriptionClient client = _cluster.ServiceProvider.GetRequiredService<SubscriptionClient>();

        Option<SubscriptionModel> result = await client.Get(nameId, context);
        if (result.IsOk()) await client.Delete(nameId, context);

        var subscription = new SubscriptionModel
        {
            SubscriptionId = IdTool.CreateSubscriptionId(nameId),
            Name = nameId,
            ContactName = nameId + "contact",
            Email = "user1@company1.com",
            Enabled = true,
        };

        Option setOption = await client.Set(subscription, context);
        setOption.StatusCode.IsOk().Should().BeTrue();

        return subscription;
    }

    public async Task<Option> DeleteSubscription(string nameId, ScopeContext context)
    {
        SubscriptionClient client = _cluster.ServiceProvider.GetRequiredService<SubscriptionClient>();

        Option deleteOption = await client.Delete(nameId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }

    public async Task<Option<TenantModel>> CreateTenant(string nameId, string subscriptionId, ScopeContext context)
    {
        TenantClient client = _cluster.ServiceProvider.GetRequiredService<TenantClient>();

        Option<TenantModel> result = await client.Get(nameId, context);
        if (result.IsOk()) await client.Delete(nameId, context);

        var tenant = new TenantModel
        {
            TenantId = IdTool.CreateTenantId(nameId),
            Domain = nameId,
            SubscriptionId = IdTool.CreateSubscriptionId(subscriptionId),
            ContactName = nameId + "contact",
            Email = "user1@company2.com",

            Enabled = true,
        };

        Option setOption = await client.Set(tenant, context);
        setOption.IsOk().Should().BeTrue();

        return tenant;
    }

    public async Task<Option> DeleteTenant(string tenantId, ScopeContext context)
    {
        TenantClient client = _cluster.ServiceProvider.GetRequiredService<TenantClient>();

        Option deleteOption = await client.Delete(tenantId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }

    public async Task CreateUser(string subscriptionId, string tenantId, string principalId)
    {
        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();

        var subscription = await CreateSubscription(subscriptionId, _context);
        subscription.IsOk().Should().BeTrue();

        var tenant = await CreateTenant(tenantId, subscriptionId, _context);
        tenant.IsOk().Should().BeTrue();

        string userId = "user:" + principalId;
        var user = await CreateUser(principalId, _context);

        Option<UserModel> readOption = await client.Get(userId, _context);
        readOption.IsOk().Should().BeTrue();
    }

    public async Task DeleteUser(string subscriptionId, string tenantId, string principalId)
    {
        string userId = "user:" + principalId;
        Option deleteOption = await DeleteUser(userId, _context);
        deleteOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteTenantOption = await DeleteTenant(tenantId, _context);
        deleteTenantOption.StatusCode.IsOk().Should().BeTrue();

        Option deleteSubscriptionOption = await DeleteSubscription(subscriptionId, _context);
        deleteSubscriptionOption.StatusCode.IsOk().Should().BeTrue();
    }


    private async Task CreateBankAccount(string accountId, string principalId, params string[] writeAccessPrincipalIds)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var existOption = await softBankClient.Exist(accountId, _context);
        if (existOption.IsOk()) await softBankClient.Delete(accountId, _context);

        var createRequest = new SbAccountDetail
        {
            AccountId = accountId,
            OwnerId = principalId,
            Name = "test account",
            AccessRights = writeAccessPrincipalIds
                .Select(x => new AccessBlock { BlockType = nameof(SbLedgerItem), PrincipalId = x, Grant = BlockGrant.Write })
                .ToArray(),
        };

        var createOption = await softBankClient.Create(createRequest, _context);
        createOption.IsOk().Should().BeTrue();

        var readAccountDetailOption = await softBankClient.GetAccountDetail(accountId, principalId, _context);
        readAccountDetailOption.IsOk().Should().BeTrue();

        var readAccountDetail = readAccountDetailOption.Return();
        (createRequest = readAccountDetail).Should().NotBeNull();
    }

    private async Task DeleteBankAccount(string accountId)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var deleteOption = await softBankClient.Delete(accountId, _context);
    }

    public async Task<Option<UserCreateModel>> CreateUser(string principalId, ScopeContext context)
    {
        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();

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

        return user;
    }

    public async Task<Option> DeleteUser(string principalId, ScopeContext context)
    {
        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();

        Option deleteOption = await client.Delete(principalId, context);
        (deleteOption.IsOk() || deleteOption.IsNotFound()).Should().BeTrue();

        return StatusCode.OK;
    }
}
