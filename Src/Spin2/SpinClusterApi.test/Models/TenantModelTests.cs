using FluentAssertions;
using SpinCluster.sdk.Actors.Tenant;
using Toolbox.Types;

namespace SpinClusterApi.test.Models;

public class TenantModelTests
{
    [Fact]
    public void TenantModelFullTest()
    {
        var model = new TenantModel()
        {
            TenantId = "tenant:company1.com",
            Domain = "company1.com",
            SubscriptionId = "subscription:company1",
            ContactName = "contact name",
            Email = "admin@company1.com",
        };

        var v = model.Validate();
        v.IsOk().Should().BeTrue(v.Error);
    }

    [Fact]
    public void MissingDomainId()
    {
        var model = new TenantModel()
        {
            TenantId = "tenant:company1.com",
            //Domain = "company1.com",
            SubscriptionId = "subscription:company1",
            ContactName = "contact name",
            Email = "admin@company1.com",
        };

        var v = model.Validate();
        v.IsError().Should().BeTrue(v.Error);
    }

    [Fact]
    public void MissingSubscription()
    {
        var model = new TenantModel()
        {
            TenantId = "tenant:company1.com",
            Domain = "company1.com",
            //SubscriptionId = "subscription:company1",
            ContactName = "contact name",
            Email = "admin@company1.com",
        };

        var v = model.Validate();
        v.IsError().Should().BeTrue(v.Error);
    }
}
