using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.test.Schema;

public class AccountRecordSerializationTests
{
    [Fact]
    public void SimpleValidation()
    {
        var rec = new AccountRecord
        {
            PrincipalId = "principalId",
            Name = "name",
        };

        var option = rec.Validate();
        option.IsOk().BeTrue();

        var json = rec.ToJson();
        var rec2 = json.ToObject<AccountRecord>();
        rec2.NotNull();
        (rec == rec2).BeTrue();
    }

    [Fact]
    public void Validation()
    {
        var rec = new AccountRecord
        {
            PrincipalId = "principalId",
            Name = "name",
            ContactItems = new[]
            {
                new ContactRecord { Type = ContactType.Email, Value = "email" },
            }.ToImmutableArray(),
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
            }.ToImmutableArray(),
            CalendarItems = new[]
            {
                new CalendarRecord
                {
                    Type = CalendarRecordType.Busy,
                    FromDate = DateTime.Now,
                    ToDate = DateTime.Now.AddDays(1),
                },
            }.ToImmutableArray(),
        };

        var option = rec.Validate();
        option.IsOk().BeTrue();

        var json = rec.ToJson();
        var rec2 = json.ToObject<AccountRecord>();
        rec2.NotNull();
        (rec == rec2).BeTrue();
    }
}