using System.Collections.Immutable;
using FluentAssertions;
using Toolbox.Extensions;
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
        option.IsOk().Should().BeTrue();

        var json = rec.ToJson();
        var rec2 = json.ToObject<AccountRecord>();
        rec2.Should().NotBeNull();
        (rec == rec2).Should().BeTrue();
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
            Address = new[]
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
        option.IsOk().Should().BeTrue();

        var json = rec.ToJson();
        var rec2 = json.ToObject<AccountRecord>();
        rec2.Should().NotBeNull();
        (rec == rec2).Should().BeTrue();
    }
}