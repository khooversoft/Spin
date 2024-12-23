using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;
using TicketShareWeb.Components.Pages.Profile.Models;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record AccountModel : IEquatable<AccountModel?>
{
    public string Name { get; set; } = "";

    public ConcurrentDictionary<string, ContactModel> ContactItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, AddressModel> AddressItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, CalendarModel> CalendarItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Equals(AccountModel? other)
    {
        return other is not null &&
            Name == other.Name &&
            ContactItems.DeepEquals(other.ContactItems) &&
            AddressItems.DeepEquals(other.AddressItems) &&
            CalendarItems.DeepEquals(other.CalendarItems);
    }

    public override int GetHashCode() => HashCode.Combine(Name, ContactItems, AddressItems, CalendarItems);
}

public static class InputModelExtensions
{
    public static AccountModel Clone(this AccountModel subject) => new AccountModel
    {
        Name = subject.Name,
        ContactItems = subject.ContactItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
        AddressItems = subject.AddressItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
        CalendarItems = subject.CalendarItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
    };

    public static AccountRecord ConvertTo(this AccountModel subject, string principalId)
    {
        subject.NotNull();

        return new AccountRecord
        {
            PrincipalId = principalId.NotEmpty(),
            Name = subject.Name,
            ContactItems = subject.ContactItems?.Values?.Select(x => x.ConvertTo())?.ToImmutableArray() ?? [],
            AddressItems = subject.AddressItems?.Values?.Select(x => x.ConvertTo())?.ToImmutableArray() ?? [],
            CalendarItems = subject.CalendarItems?.Values?.Select(x => x.ConvertTo()).ToImmutableArray() ?? [],
        };
    }

    public static AccountModel ConvertTo(this AccountRecord subject)
    {
        subject.NotNull();

        return new AccountModel
        {
            Name = subject.Name.NotEmpty(),
            ContactItems = subject.ContactItems
                .Select(x => x.ConvertTo().ToKeyValuePair(x.Id))
                .ToConcurrentDictionary(StringComparer.OrdinalIgnoreCase),

            AddressItems = subject.AddressItems
                .Select(x => x.ConvertTo().ToKeyValuePair(x.Id))
                .ToConcurrentDictionary(StringComparer.OrdinalIgnoreCase),

            CalendarItems = subject.CalendarItems
                .Select(x => x.ConvertTo().ToKeyValuePair(x.Id))
                .ToConcurrentDictionary(StringComparer.OrdinalIgnoreCase),
        };
    }
}
