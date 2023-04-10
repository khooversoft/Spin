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
//    public class TenantControllerTests
//    {
//        [Fact]
//        public async Task GivenData_WhenRoundTrip_ShouldMatch()
//        {
//            IdentityTestHost host = TestApplication.GetHost();

//            var tenant = new Tenant
//            {
//                TenantId = new IdentityId("tenant_01"),
//                Name = "Tenant #1"
//            };

//            await host.IdentityClient.Tenant.Set(tenant);

//            Tenant? readTenant = await host.IdentityClient.Tenant.Get(tenant.TenantId);
//            readTenant.Should().NotBeNull();

//            (tenant == readTenant).Should().BeTrue();

//            BatchSet<string> searchList = await host.IdentityClient.Tenant.List(QueryParameter.Default).ReadNext();
//            searchList.Should().NotBeNull();
//            searchList.Records.Any(x => x.StartsWith((string)tenant.TenantId)).Should().BeTrue();

//            (await host.IdentityClient.Tenant.Delete(tenant.TenantId)).Should().BeTrue();

//            searchList = await host.IdentityClient.Tenant.List(QueryParameter.Default).ReadNext();
//            searchList.Should().NotBeNull();
//            searchList.Records.Any(x => x.StartsWith((string)tenant.TenantId)).Should().BeFalse();
//        }
//    }
//}
