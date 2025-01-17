//using Toolbox.Tools.Should;
//using Toolbox.Extensions;
//using Toolbox.Orleans.test.Application;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans.test.Actor;

//public class IdentityServiceTests : IClassFixture<ActorClusterFixture>
//{
//    private readonly ActorClusterFixture _actorFixture;
//    public IdentityServiceTests(ActorClusterFixture clusterFixture) => _actorFixture = clusterFixture.NotNull();

//    [Fact]
//    public async Task AddIdentity()
//    {
//        var service = _actorFixture.Cluster.Client.GetIdentityActor();

//        var userIdentity = new PrincipalIdentity
//        {
//            PrincipalId = "User001",
//            UserName = "userName1",
//            Email = "userName1@domain.com",
//            LoginProvider = "ms",
//            ProviderKey = "providerKey",
//        };

//        userIdentity.Validate().IsOk().Should().BeTrue();
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
//        var service = _actorFixture.Cluster.Client.GetIdentityActor();

//        const string userId1 = "user001";
//        const string userName1 = "userName1";
//        const string userId2 = "user002";
//        const string userName2 = "userName2";

//        await addIdentity(userId1, userName1, "userName1@domain.com", "providerkey1");
//        await addIdentity(userId2, userName2, "userName2@domain.com", "providerkey2");

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

//        (await service.Delete(userId1, NullScopeContext.Instance)).IsOk().Should().BeTrue();
//        (await service.Delete(userId2, NullScopeContext.Instance)).IsOk().Should().BeTrue();

//        async Task addIdentity(string userId, string userName, string email, string providerKey)
//        {
//            var userIdentity = new PrincipalIdentity
//            {
//                PrincipalId = userId,
//                UserName = userName,
//                Email = email,
//                LoginProvider = "ms",
//                ProviderKey = providerKey,
//            };

//            userIdentity.Validate().IsOk().Should().BeTrue();
//            await service.Delete(userIdentity.PrincipalId, NullScopeContext.Instance);

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