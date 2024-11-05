using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TicketShare.sdk.Applications;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk.test.Models;

public class AccountRecordTests
{
    [Fact]
    public async Task FullLifeCycle()
    {
        var testHost = new TicketShareTestHost();
        var client = testHost.ServiceProvider.GetRequiredService<AccountClient>();
        var context = testHost.GetScopeContext<AccountRecordTests>();

        var accountRecord = TestTool.Create("user1@domain.com");
        await TestTool.AddIdentityUser(accountRecord.PrincipalId, "user1", testHost, context);

        var result = await client.Add(accountRecord, context);
        result.IsOk().Should().BeTrue(result.ToString());

        var readAccount = await client.Get(accountRecord.PrincipalId, context);
        readAccount.IsOk().Should().BeTrue();

        (accountRecord == readAccount.Return()).Should().BeTrue();

        accountRecord = accountRecord with
        {
            ContactItems = accountRecord.ContactItems
                .Append(new ContactRecord { Type = ContactType.Cell, Value = "202-555-1212" })
                .ToArray(),
        };

        result = await client.Set(accountRecord, context);
        result.IsOk().Should().BeTrue();

        readAccount = await client.Get(accountRecord.PrincipalId, context);
        readAccount.IsOk().Should().BeTrue();
        (accountRecord == readAccount.Return()).Should().BeTrue();

        var delete = await client.Delete(accountRecord.PrincipalId, context);
        delete.IsOk().Should().BeTrue();

        readAccount = await client.Get(accountRecord.PrincipalId, context);
        readAccount.IsError().Should().BeTrue();
    }
}
