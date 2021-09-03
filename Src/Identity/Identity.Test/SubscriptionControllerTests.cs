using FluentAssertions;
using Identity.sdk.Models;
using Identity.sdk.Types;
using Identity.Test.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Model;
using Xunit;

namespace Identity.Test
{
    public class SubscriptionControllerTests
    {
        [Fact]
        public async Task GivenData_WhenRoundTrip_ShouldMatch()
        {
            IdentityTestHost host = TestApplication.GetHost();

            var subscription = new Subscription
            {
                TenantId = new IdentityId("tenant_01"),
                SubscriptionId = new IdentityId("subscription_01"),
                Name = "Subscription #1"
            };

            await host.IdentityClient.Subscription.Set(subscription);

            Subscription? readItem = await host.IdentityClient.Subscription.Get(subscription.TenantId, subscription.SubscriptionId);
            readItem.Should().NotBeNull();

            (subscription== readItem).Should().BeTrue();

            BatchSet<string> searchList = await host.IdentityClient.Subscription.List(QueryParameter.Default).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith((string)subscription.TenantId)).Should().BeTrue();

            (await host.IdentityClient.Subscription.Delete(subscription.TenantId, subscription.SubscriptionId)).Should().BeTrue();

            searchList = await host.IdentityClient.Subscription.List(QueryParameter.Default).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith((string)subscription.TenantId)).Should().BeFalse();
        }
    }
}
