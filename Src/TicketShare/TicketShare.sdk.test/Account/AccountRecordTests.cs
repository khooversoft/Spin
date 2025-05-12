using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.test.Account;

public class AccountRecordTests
{
    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = await GraphTestStartup.CreateGraphService();
        var client = testHost.Services.GetRequiredService<AccountClient>();
        IGraphClient graphClient = testHost.Services.GetRequiredService<IGraphClient>();
        var context = testHost.CreateScopeContext<AccountRecordTests>();

        var accountRecord = TestTool.CreateAccountModel("user1@domain.com");
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);

        var result = await client.GetContext(accountRecord.PrincipalId).Add(accountRecord, context);
        result.IsOk().BeTrue(result.ToString());

        var readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().BeTrue();

        (accountRecord == readAccount.Return()).BeTrue();

        string accountKey = AccountTool.ToNodeKey(accountRecord.PrincipalId);
        var queryResult = await graphClient.Execute($"select (key={accountKey}) -> [*] ;", context);
        queryResult.IsOk().BeTrue();
        queryResult.Return().Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.Edges[0].Action(y =>
            {
                y.FromKey.Be(accountKey);
                y.ToKey.Be("user:user1@domain.com");
                y.EdgeType.Be("account-owns");
            });
        });

        accountRecord = accountRecord with
        {
            ContactItems = accountRecord.ContactItems
                .Append(new ContactRecord { Type = ContactType.Phone, Value = "202-555-1212" })
                .ToArray(),
        };

        result = await client.GetContext(accountRecord.PrincipalId).Set(accountRecord, context);
        result.IsOk().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().BeTrue();
        (accountRecord == readAccount.Return()).BeTrue();

        var delete = await client.GetContext(accountRecord.PrincipalId).Delete(context);
        delete.IsOk().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsError().BeTrue();
    }

    [Fact]
    public async Task FullLifeCycleWithCreate()
    {
        var testHost = await GraphTestStartup.CreateGraphService();
        var client = testHost.Services.GetRequiredService<AccountClient>();
        IGraphClient graphClient = testHost.Services.GetRequiredService<IGraphClient>();
        var context = testHost.CreateScopeContext<AccountRecordTests>();

        const string principalId = "user1@domain.com";
        await TestTool.AddIdentityUser(principalId, "user1", testHost, context);

        var result = await client.Create(principalId, context);
        result.IsOk().BeTrue(result.ToString());
        AccountRecord accountRecord = result.Return();

        var readAccount = await client.GetContext(principalId).Get(context);
        readAccount.IsOk().BeTrue();

        (accountRecord == readAccount.Return()).BeTrue();

        string accountKey = AccountTool.ToNodeKey(accountRecord.PrincipalId);
        var queryResult = await graphClient.Execute($"select (key={accountKey}) -> [*] ;", context);
        queryResult.IsOk().BeTrue();
        queryResult.Return().Action(x =>
        {
            x.Nodes.Count.Be(0);
            x.Edges.Count.Be(1);
            x.Edges[0].Action(y =>
            {
                y.FromKey.Be(accountKey);
                y.ToKey.Be("user:user1@domain.com");
                y.EdgeType.Be("account-owns");
            });
        });

        accountRecord = accountRecord with
        {
            ContactItems = accountRecord.ContactItems
                .Append(new ContactRecord { Type = ContactType.Phone, Value = "202-555-1212" })
                .ToArray(),
        };

        var setOption = await client.GetContext(accountRecord.PrincipalId).Set(accountRecord, context);
        setOption.IsOk().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsOk().BeTrue();
        (accountRecord == readAccount.Return()).BeTrue();

        var delete = await client.GetContext(accountRecord.PrincipalId).Delete(context);
        delete.IsOk().BeTrue();

        readAccount = await client.GetContext(accountRecord.PrincipalId).Get(context);
        readAccount.IsError().BeTrue();
    }
}
