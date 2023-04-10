//using FluentAssertions;
//using Identity.sdk.Models;
//using Identity.sdk.Types;
//using Identity.Test.Application;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Model;
//using Xunit;

//namespace Identity.Test
//{
//    public class UserControllerTest
//    {
//        [Fact]
//        public async Task GivenData_WhenRoundTrip_ShouldMatch()
//        {
//            IdentityTestHost host = TestApplication.GetHost();

//            var user = new User
//            {
//                TenantId = new IdentityId("tenant_01"),
//                SubscriptionId = new IdentityId("subscription_01"),
//                UserId = new UserId("user01@domain.com"),
//                Name = "User #1",
//                PublicSignaturesId = new[] { "key_1" }.ToArray()
//            };

//            await host.IdentityClient.User.Set(user);

//            User? readItem = await host.IdentityClient.User.Get(user.TenantId, user.SubscriptionId, user.UserId);
//            readItem.Should().NotBeNull();

//            (user.TenantId == readItem!.TenantId).Should().BeTrue();
//            (user.SubscriptionId == readItem.SubscriptionId).Should().BeTrue();
//            (user.UserId == readItem.UserId).Should().BeTrue();
//            (user.Name == readItem.Name).Should().BeTrue();
//            readItem.PublicSignaturesId!.Count.Should().Be(1);
//            (user.PublicSignaturesId[0] == readItem.PublicSignaturesId[0]).Should().BeTrue();

//            BatchSet<string> searchList = await host.IdentityClient.User.List(QueryParameter.Default).ReadNext();
//            searchList.Should().NotBeNull();
//            searchList.Records.Any(x => x.StartsWith((string)user.TenantId)).Should().BeTrue();

//            (await host.IdentityClient.User.Delete(user.TenantId, user.SubscriptionId, user.UserId)).Should().BeTrue();

//            searchList = await host.IdentityClient.User.List(QueryParameter.Default).ReadNext();
//            searchList.Should().NotBeNull();
//            searchList.Records.Any(x => x.StartsWith((string)user.TenantId)).Should().BeFalse();
//        }
//    }
//}
