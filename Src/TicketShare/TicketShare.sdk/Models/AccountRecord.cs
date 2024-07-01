using System.Collections.Frozen;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public record AccountRecord
{
    [Id(0)] public string PrincipalId { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;

    [Id(2)] public FrozenSet<ContactRecord> ContactItems { get; init; } = FrozenSet<ContactRecord>.Empty;
    [Id(3)] public FrozenSet<AddressRecord> Address { get; init; } = FrozenSet<AddressRecord>.Empty;
    [Id(4)] public FrozenSet<CalendarRecord> CalendarItems { get; init; } = FrozenSet<CalendarRecord>.Empty;

    public static IValidator<AccountRecord> Validator { get; } = new Validator<AccountRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.ContactItems).Validate(ContactRecord.Validator)
        .RuleForEach(x => x.CalendarItems).Validate(CalendarRecord.Validator)
        .Build();
}

public static class AccountRecordExtensions
{
    public static Option Validate(this AccountRecord subject) => AccountRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AccountRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static AccountRecord Merge(this AccountRecord subject, IEnumerable<ContactRecord> contactRecords)
    {
        contactRecords.NotNull();

        subject = subject with
        {
            ContactItems = subject.ContactItems
                .Where(x => !contactRecords.Any(y => y.Type != x.Type))
                .Concat(contactRecords)
                .ToFrozenSet()
        };

        return subject;
    }

    public static AccountRecord Merge(this AccountRecord subject, IEnumerable<AddressRecord> addressRecords)
    {
        addressRecords.NotNull();

        subject = subject with
        {
            Address = subject.Address
                .Where(x => !addressRecords.Any(y => y.IsMatch(x)))
                .Concat(addressRecords)
                .ToFrozenSet()
        };

        return subject;
    }
}