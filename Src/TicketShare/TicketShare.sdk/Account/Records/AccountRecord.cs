using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

/// <summary>
/// PrincipalId can only own 1 account record
/// All users must have an account
/// </summary>
public sealed record AccountRecord
{
    public string PrincipalId { get; init; } = null!;   // Owner
    public string? Name { get; init; }

    public IReadOnlyList<ContactRecord> ContactItems { get; init; } = Array.Empty<ContactRecord>();
    public IReadOnlyList<AddressRecord> AddressItems { get; init; } = Array.Empty<AddressRecord>();
    public IReadOnlyList<CalendarRecord> CalendarItems { get; init; } = Array.Empty<CalendarRecord>();

    public bool Equals(AccountRecord? obj) => obj is AccountRecord subject &&
        PrincipalId == subject.PrincipalId &&
        Name == subject.Name &&
        Enumerable.SequenceEqual(ContactItems, subject.ContactItems) &&
        Enumerable.SequenceEqual(AddressItems, subject.AddressItems) &&
        Enumerable.SequenceEqual(CalendarItems, subject.CalendarItems);

    public override int GetHashCode() => HashCode.Combine(PrincipalId, Name, ContactItems, AddressItems, CalendarItems);

    public static IValidator<AccountRecord> Validator { get; } = new Validator<AccountRecord>()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleForEach(x => x.ContactItems).Validate(ContactRecord.Validator)
        .RuleForEach(x => x.AddressItems).Validate(AddressRecord.Validator)
        .RuleForEach(x => x.CalendarItems).Validate(CalendarRecord.Validator)
        .Build();
}

public static class AccountRecordTool
{
    public static Option Validate(this AccountRecord subject) => AccountRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AccountRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static AccountRecord Merge(this AccountRecord subject, IEnumerable<ContactRecord> contactRecords)
    {
        subject.NotNull();
        contactRecords.NotNull();

        subject = subject with
        {
            ContactItems = subject.ContactItems
                .Where(x => !contactRecords.Any(y => y.Id != x.Id))
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
            AddressItems = currentRecord.AddressItems
                .Where(x => !newRecords.Any(y => x.Id.EqualsIgnoreCase(y.Id)))
                .Concat(newAddressRecords)
                .ToImmutableArray(),
        };

        return currentRecord;
    }
}