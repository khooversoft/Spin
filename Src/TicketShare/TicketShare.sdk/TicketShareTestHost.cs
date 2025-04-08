//using System.Security.Claims;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public class TicketShareTestHost
//{
//    private GraphTestClient _testClient;

//    public TicketShareTestHost(string? principalId = null)
//    {
//        _testClient = GraphTestStartup.CreateGraphTestHost(null, service =>
//        {
//            service.AddTicketShare();
//            service.AddSingleton<AuthenticationStateProvider>(s =>
//            {
//                return principalId.IsEmpty() ? new TestAuthStateProvider() : new TestAuthStateProvider(principalId);
//            });
//        });
//    }

//    public IGraphClient TestClient => _testClient;

//    public IServiceProvider ServiceProvider => _testClient.ServiceProvider;
//    public ScopeContext GetScopeContext<T>() => _testClient.GetScopeContext<T>();
//}

//public class TestAuthStateProvider : AuthenticationStateProvider
//{
//    private readonly string _principalId = "user1@domain.com";

//    public TestAuthStateProvider() { }
//    public TestAuthStateProvider(string principalId) => _principalId = principalId.NotEmpty();

//    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.Name, _principalId),
//            new Claim(ClaimTypes.Role, "Administrator")
//        };
//        var anonymous = new ClaimsIdentity(claims, "testAuthType");

//        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
//    }
//}