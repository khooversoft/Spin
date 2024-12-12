using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;
using Toolbox.Tools;
using Toolbox.Extensions;
using TicketShareWeb.Components.Pages.Profile.Models;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record InputModel : IEquatable<InputModel?>
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    public ConcurrentDictionary<string, ContactModel> ContactItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, AddressModel> AddressItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, CalendarModel> CalendarItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Equals(InputModel? other)
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
    public static InputModel Clone(this InputModel subject) => new InputModel
    {
        Name = subject.Name,
        ContactItems = subject.ContactItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
        AddressItems = subject.AddressItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
        CalendarItems = subject.CalendarItems.Clone(x => x.Clone().ToKeyValuePair(x.Id)),
    };

    public static AccountRecord ConvertTo(this InputModel subject, string principalId)
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

    public static InputModel ConvertTo(this AccountRecord subject)
    {
        subject.NotNull();

        return new InputModel
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
