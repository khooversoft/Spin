using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder;

public record TenantInfo(string Subscription, string Tenant);
public record AccountInfo(string AccountId, string PrincipalId, string[] WriteAccess);

public class ObjectOptionBuilder
{
    public List<string> Subscriptions { get; init; } = new List<string>();
    public List<TenantInfo> Tenants { get; init; } = new List<TenantInfo>();
    public List<string> Users { get; init; } = new List<string>();
    public List<AccountInfo> Accounts { get; init; } = new List<AccountInfo>();

    public ObjectOptionBuilder AddSubscription(string subscription) => this.Action(_ => Subscriptions.Add(subscription.NotEmpty()));
    public ObjectOptionBuilder AddTenant(string subscription, string tenant) => this.Action(_ => Tenants.Add(new TenantInfo(subscription.NotEmpty(), tenant.NotEmpty())));
    public ObjectOptionBuilder AddUser(string principalId) => this.Action(_ => Users.Add(principalId.NotEmpty()));
    public ObjectOptionBuilder AddAccount(string accountId, string principalId, params string[] writeAccess) =>
        this.Action(_ => Accounts.Add(new AccountInfo(accountId.NotEmpty(), principalId.NotEmpty(), writeAccess)));

    public ObjectBuilderOption Build()
    {
        var option = new ObjectBuilderOption
        {
            Subscriptions = Subscriptions.Select(x => new SubscriptionModel
            {
                SubscriptionId = "subscription:" + x,
                Name = x,
                ContactName = x + " contract",
                Email = "user1@domain.com",
                Enabled = true,
            }).ToList(),

            Tenants = Tenants.Select(x => new TenantModel
            {
                TenantId = "tenant:" + x.Tenant,
                Domain = x.Tenant,
                SubscriptionId = "subscription:" + x.Subscription,
                ContactName = "contact name",
                Email = "user@" + x.Tenant,
                Enabled = true,

            }).ToList(),

            Users = Users.Select(x => new UserCreateModel
            {
                UserId = "user:" + x,
                PrincipalId = x,
                DisplayName = "Display name for " + x,
                FirstName = "firstName",
                LastName = "lastName",
            }).ToList(),

            Accounts = Accounts.Select(x => new AccountDetail
            {
                AccountId = x.AccountId,
                OwnerId = x.PrincipalId,
                Name = "test account",
                AccessRights = x.WriteAccess
                    .Select(x => new AccessBlock { BlockType = nameof(LedgerItem), PrincipalId = x, Grant = BlockGrant.ReadWrite })
                    .ToArray(),
            }).ToList(),
        };

        return option;
    }
}