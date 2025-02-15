using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace TicketShare.sdk.test.Account;

public class AccountRecordTests
{
    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<AccountClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<AccountRecordTests>();

        var accountRecord = TestTool.CreateAccountModel("user1@domain.com");
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);

        var result = await client.GetContext(accountRecord.PrincipalId).Add(accountRecord, context);
        result.IsOk().Should().BeTrue(result.ToString());

        var readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().Should().BeTrue();

        (accountRecord == readAccount.Return()).Should().BeTrue();

        string accountKey = AccountTool.ToNodeKey(accountRecord.PrincipalId);
        var queryResult = await graphClient.Execute($"select (key={accountKey}) -> [*] ;", context);
        queryResult.IsOk().Should().BeTrue();
        queryResult.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.Edges[0].Action(y =>
            {
                y.FromKey.Should().Be(accountKey);
                y.ToKey.Should().Be("user:user1@domain.com");
                y.EdgeType.Should().Be("account-owns");
            });
        });

        accountRecord = accountRecord with
        {
            ContactItems = accountRecord.ContactItems
                .Append(new ContactRecord { Type = ContactType.Phone, Value = "202-555-1212" })
                .ToArray(),
        };

        result = await client.GetContext(accountRecord.PrincipalId).Set(accountRecord, context);
        result.IsOk().Should().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().Should().BeTrue();
        (accountRecord == readAccount.Return()).Should().BeTrue();

        var delete = await client.GetContext(accountRecord.PrincipalId).Delete(context);
        delete.IsOk().Should().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsError().Should().BeTrue();
    }
    
    [Fact]
    public async Task FullLifeCycleWithCreate()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<AccountClient>();
        IGraphClient graphClient = testHost.ServiceProvider.GetRequiredService<IGraphClient>();
        var context = testHost.GetScopeContext<AccountRecordTests>();

        const string principalId = "user1@domain.com";
        await TestTool.AddIdentityUser(principalId, "user1", testHost, context);

        var result = await client.Create(principalId, context);
        result.IsOk().Should().BeTrue(result.ToString());
        AccountRecord accountRecord = result.Return();

        var readAccount = await client.GetContext(principalId).Get(context);
        readAccount.IsOk().Should().BeTrue();

        (accountRecord == readAccount.Return()).Should().BeTrue();

        string accountKey = AccountTool.ToNodeKey(accountRecord.PrincipalId);
        var queryResult = await graphClient.Execute($"select (key={accountKey}) -> [*] ;", context);
        queryResult.IsOk().Should().BeTrue();
        queryResult.Return().Action(x =>
        {
            x.Nodes.Count.Should().Be(0);
            x.Edges.Count.Should().Be(1);
            x.Edges[0].Action(y =>
            {
                y.FromKey.Should().Be(accountKey);
                y.ToKey.Should().Be("user:user1@domain.com");
                y.EdgeType.Should().Be("account-owns");
            });
        });

        accountRecord = accountRecord with
        {
            ContactItems = accountRecord.ContactItems
                .Append(new ContactRecord { Type = ContactType.Phone, Value = "202-555-1212" })
                .ToArray(),
        };

        var setOption = await client.GetContext(accountRecord.PrincipalId).Set(accountRecord, context);
        setOption.IsOk().Should().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().Should().BeTrue();
        (accountRecord == readAccount.Return()).Should().BeTrue();

        var delete = await client.GetContext(accountRecord.PrincipalId).Delete(context);
        delete.IsOk().Should().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsError().Should().BeTrue();
    }
}
