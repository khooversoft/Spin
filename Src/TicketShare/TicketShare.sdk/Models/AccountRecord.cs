using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Extensions;

namespace TicketShare.sdk;

[GenerateSerializer]
public record AccountRecord
{
    [Id(0)] public string PrincipalId { get; init; } = null!;
    [Id(1)] public string Name { get; init; } = null!;

    [Id(2)] public IReadOnlyList<ContactRecord> ContactItems { get; init; } = ImmutableArray<ContactRecord>.Empty;
    [Id(3)] public IReadOnlyList<AddressRecord> Address { get; init; } = ImmutableArray<AddressRecord>.Empty;
    [Id(4)] public IReadOnlyList<CalendarRecord> CalendarItems { get; init; } = ImmutableArray<CalendarRecord>.Empty;

    public static IValidator<AccountRecord> Validator { get; } = new Validator<AccountRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.ContactItems).Validate(ContactRecord.Validator)
        .RuleForEach(x => x.Address).Validate(AddressRecord.Validator)
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
                .ToImmutableArray(),
        };

        return subject;
    }

    public static AccountRecord Merge(this AccountRecord currentRecord, IEnumerable<AddressRecord> newAddressRecords)
    {
        newAddressRecords.NotNull();

        var newRecords = newAddressRecords
            .GroupBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();

        currentRecord = currentRecord with
        {
            Address = currentRecord.Address
                .Where(x => newRecords.Any(y => x.Label.EqualsIgnoreCase(y.Label)))
                .Concat(newAddressRecords)
                .ToImmutableArray(),
        };

        return currentRecord;
    }
}