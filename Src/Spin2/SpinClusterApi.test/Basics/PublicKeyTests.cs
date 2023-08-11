//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging.Abstractions;
//using SpinCluster.sdk.Actors.Key;
//using SpinCluster.sdk.Actors.PrincipalKey;
//using SpinCluster.sdk.Actors.User;
//using SpinCluster.sdk.Application;
//using SpinClusterApi.test.Application;
//using Toolbox.Types;

//namespace SpinClusterApi.test.Basics;

//public class PublicKeyTests : IClassFixture<ClusterApiFixture>
//{
//    private readonly ClusterApiFixture _cluster;
//    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

//    public PublicKeyTests(ClusterApiFixture fixture)
//    {
//        _cluster = fixture;
//    }

//    [Fact(Skip = "Go to API")]
//    public async Task TestReadWrite()
//    {
//        string ownerId = "signKey@test.com";
//        string keyId = $"{SpinConstants.Schema.PrincipalKey}/test/TestReadWrite/{ownerId}";

//        UserClient client = _cluster.ServiceProvider.GetRequiredService<UserClient>();


//        IPrincipalKeyActor actor = _cluster.GrainFactory.GetGrain<IPrincipalKeyActor>(keyId);

//        await actor.Delete(_context.TraceId);

//        var rsaKey = new RsaKeyPair("key");
//        var request = new PrincipalKeyModel
//        {
//            KeyId = keyId,
//            PrincipalId = ownerId,
//            Audience = "test.com",
//            Name = "test sign key",
//            PublicKey = rsaKey.PublicKey,
//        };

//        Option writeResult = await actor.Set(request, _context.TraceId);
//        writeResult.StatusCode.IsOk().Should().BeTrue();

//        Option<PrincipalKeyModel> read = await actor.Get(_context.TraceId);
//        read.StatusCode.IsOk().Should().BeTrue();

//        (request == read.Return()).Should().BeTrue();

//        await actor.Delete(_context.TraceId);
//    }
//}