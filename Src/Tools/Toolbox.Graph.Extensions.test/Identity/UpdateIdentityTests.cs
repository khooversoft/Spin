//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Xunit.Abstractions;

//namespace Toolbox.Graph.Extensions.test.Identity;

//public class UpdateIdentityTests
//{
//    private ITestOutputHelper _outputHelper;
//    public UpdateIdentityTests(ITestOutputHelper outputHelper) => _outputHelper = outputHelper.NotNull();

//    [Fact]
//    public async Task UpdateUserEmailInfo()
//    {
//        using var graphTestClient = await GraphTestStartup.CreateGraphService(config: x => x.AddGraphExtensions());
//        var context = graphTestClient.CreateScopeContext<UpdateIdentityTests>();
//        var identityClient = graphTestClient.Services.GetRequiredService<IdentityClient>();

//        var userId = "userName1@company.com";
//        var userEmail = "userName1@domain1.com";
//        var userName = "userName1";
//        var loginProvider = "loginProviderNet";
//        var providerKey = "loginProvider.key1";

//        var user = new PrincipalIdentity
//        {
//            PrincipalId = userId,
//            UserName = userName,
//            Email = userEmail,
//            NormalizedUserName = userName.ToLower(),
//            Name = "user name",
//            LoginProvider = loginProvider,
//            ProviderKey = providerKey,
//            ProviderDisplayName = "testProvider",
//        };

//        user.Validate().IsOk().BeTrue();

//        await TestTool.CreateAndVerify(user, graphTestClient, context);
//        graphTestClient.Map.Nodes.Count.Be(1);
//        graphTestClient.Map.Edges.Count.Be(0);

//        var list = new (string indexName, string key)[]
//        {
//            ("email", "username1@domain1.com"),
//            ("loginProvider", "loginprovidernet/loginprovider.key1"),
//            ("userName", "userName1"),
//        };

//        VerifyIndex(graphTestClient, list);

//        var updatedUser = user with { Email = "newUserName@domainNew.com" };

//        await TestTool.CreateAndVerify(updatedUser, graphTestClient, context);
//        graphTestClient.Map.Nodes.Count.Be(1);
//        graphTestClient.Map.Edges.Count.Be(0);

//        list = new (string indexName, string key)[]
//        {
//            ("email", "newUserName@domainNew.com"),
//            ("loginProvider", "loginprovidernet/loginprovider.key1"),
//            ("userName", "userName1"),
//        };

//        VerifyIndex(graphTestClient, list);

//        var deleteResult = await identityClient.Delete(userId, context);
//        deleteResult.IsOk().BeTrue();
//        graphTestClient.Map.Nodes.Count.Be(0);
//        graphTestClient.Map.Edges.Count.Be(0);

//        var selectCmd = new SelectCommandBuilder().AddNodeSearch(x => x.SetNodeKey(userId)).Build();

//        var deleteOption = await graphTestClient.Execute(selectCmd, context);
//        deleteOption.IsOk().BeTrue();
//        deleteOption.Return().Action(x =>
//        {
//            x.Nodes.Count.Be(0);
//            x.Edges.Count.Be(0);
//        });
//    }

//    //private static void VerifyIndex(GraphHostService graphHostService, (string indexName, string key)[] list)
//    //{
//    //    foreach (var item in list)
//    //    {
//    //        var r = graphHostService.Map.Nodes.LookupIndex(item.indexName, item.key);
//    //        r.IsOk().BeTrue();
//    //        var rv = r.Return();
//    //        rv.NodeKey.Be("user:username1@company.com");
//    //    }
//    //}
//}
