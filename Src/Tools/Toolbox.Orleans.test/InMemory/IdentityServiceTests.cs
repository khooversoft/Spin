//using Toolbox.Tools.Should;
//using Toolbox.Extensions;
//using Toolbox.Orleans.test.Application;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans.test.InMemory;

//public class IdentityServiceTests : IClassFixture<InMemoryClusterFixture>
//{
//    private readonly InMemoryClusterFixture _clusterFixture;
//    public IdentityServiceTests(InMemoryClusterFixture clusterFixture) => _clusterFixture = clusterFixture.NotNull();

//    [Fact]
//    public async Task AddIdentity()
//    {
//        var service = _clusterFixture.Cluster.Client.GetIdentityActor();

//        var userIdentity = new PrincipalIdentity
//        {
//            PrincipalId = Guid.NewGuid().ToString(),
//            UserName = "userName1",
//            Email = "userName1@domain.com",
//            LoginProvider = "ms",
//            ProviderKey = "providerKey",
//        };

//        userIdentity.Validate().IsOk().Should().BeTrue();

//        (await service.GetByUserName(userIdentity.UserName, NullScopeContext.Instance)).Action(x =>
//        {
//            if (x.IsOk())
//            {
//                var deleteResult = service.Delete(x.Return().PrincipalId, NullScopeContext.Instance).Result;
//                deleteResult.IsOk().Should().BeTrue();
//            }
//        });

//        await service.Delete(userIdentity.PrincipalId, NullScopeContext.Instance);

//        (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

//        (await service.GetById(userIdentity.PrincipalId, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByUserName(userIdentity.UserName, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByEmail(userIdentity.Email, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.GetByLogin(userIdentity.LoginProvider, userIdentity.ProviderKey, NullScopeContext.Instance)).Action(x =>
//        {
//            x.IsOk().Should().BeTrue();
//            (userIdentity == x.Return()).Should().BeTrue();
//        });

//        (await service.Delete(userIdentity.PrincipalId, NullScopeContext.Instance)).IsOk().Should().BeTrue();
//    }

//    [Fact]
//    public async Task AddTwoIdentity()
//    {
//        var service = _clusterFixture.Cluster.Client.GetIdentityActor();

//        const string userName1 = "userName10";
//        const string userName2 = "userName11";

//        await addIdentity(userName1, "userName10@domain.com", "providerkey1");
//        await addIdentity(userName2, "userName11@domain.com", "providerkey2");


//        (await service.GetByUserName(userName1, NullScopeContext.Instance)).Action(async x =>
//        {
//            x.IsOk().Should().BeTrue();
//            var deleteResult = await service.Delete(x.Return().PrincipalId, NullScopeContext.Instance);
//            deleteResult.IsOk().Should().BeTrue();
//        });

//        (await service.GetByUserName(userName2, NullScopeContext.Instance)).Action(async x =>
//        {
//            x.IsOk().Should().BeTrue();
//            var deleteResult = await service.Delete(x.Return().PrincipalId, NullScopeContext.Instance);
//            deleteResult.IsOk().Should().BeTrue();
//        });

//        async Task addIdentity(string userName, string email, string providerKey)
//        {
//            var userIdentity = new PrincipalIdentity
//            {
//                PrincipalId = Guid.NewGuid().ToString(),
//                UserName = userName,
//                Email = email,
//                LoginProvider = "ms",
//                ProviderKey = providerKey,
//            };

//            userIdentity.Validate().IsOk().Should().BeTrue();

//            (await service.Set(userIdentity, NullScopeContext.Instance)).IsOk().Should().BeTrue();

//            (await service.GetById(userIdentity.PrincipalId, NullScopeContext.Instance)).Action(x =>
//            {
//                x.IsOk().Should().BeTrue();
//                (userIdentity == x.Return()).Should().BeTrue();
//            });

//            (await service.GetByUserName(userIdentity.UserName, NullScopeContext.Instance)).Action(x =>
//            {
//                x.IsOk().Should().BeTrue();
//                (userIdentity == x.Return()).Should().BeTrue();
//            });

//            (await service.GetByEmail(userIdentity.Email, NullScopeContext.Instance)).Action(x =>
//            {
//                x.IsOk().Should().BeTrue();
//                (userIdentity == x.Return()).Should().BeTrue();
//            });

//            (await service.GetByLogin(userIdentity.LoginProvider, userIdentity.ProviderKey, NullScopeContext.Instance)).Action(x =>
//            {
//                x.IsOk().Should().BeTrue();
//                (userIdentity == x.Return()).Should().BeTrue();
//            });
//        }
//    }
//}