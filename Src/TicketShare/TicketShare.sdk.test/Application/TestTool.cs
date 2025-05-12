using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Applications;

internal static class TestTool
{
    public static async Task AddIdentityUser(string principalId, string userName, GraphHostService testHost, ScopeContext context)
    {
        var client = testHost.Services.GetRequiredService<IdentityClient>();

        PrincipalIdentity user = new PrincipalIdentity
        {
            PrincipalId = principalId,
            Email = "em-" + principalId,
            UserName = userName,
            Name = $"name:{userName}",
            NormalizedUserName = userName.ToLowerInvariant(),
        };

        var result = await client.Set(user, context);
        result.IsOk().BeTrue(result.ToString());
    }

    public static async Task AddAccount(AccountRecord accountRecord, GraphHostService testHost, ScopeContext context)
    {
        var client = testHost.Services.GetRequiredService<AccountClient>();

        var result = await client.GetContext(accountRecord.PrincipalId).Add(accountRecord, context);
        result.IsOk().BeTrue(result.ToString());
    }

    public static AccountRecord CreateAccountModel(string principalId)
    {
        var rec = new AccountRecord
        {
            PrincipalId = principalId,
            Name = "name",
            ContactItems = new[]
            {
                new ContactRecord { Type = ContactType.Email, Value = "email" },
            },
            AddressItems = new[]
            {
                new AddressRecord
                {
                    Label = "label",
                    Address1 = "address1",
                    City = "city",
                    State = "state",
                    ZipCode = "zipCode",
                },
            },
            CalendarItems = new[]
            {
                new CalendarRecord
                {
                    Type = CalendarRecordType.Busy,
                    FromDate = DateTime.Now,
                    ToDate = DateTime.Now.AddDays(1),
                },
            },
        };

        var option = rec.Validate();
        option.IsOk().BeTrue();

        return rec;
    }
}
