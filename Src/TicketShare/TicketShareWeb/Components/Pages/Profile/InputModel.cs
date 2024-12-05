using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace TicketShareWeb.Components.Pages.Profile;

public sealed class InputModel : IEquatable<InputModel?>
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    public ConcurrentDictionary<string, ContactModel> ContactItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, AddressModel> AddressItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, CalendarModel> CalendarItems { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as InputModel);

    public bool Equals(InputModel? other)
    {
        return other is not null &&
               Name == other.Name &&
               ContactItems.DeepEquals(other.ContactItems) &&
               AddressItems.DeepEquals(other.AddressItems) &&
               CalendarItems.DeepEquals(other.CalendarItems);
    }

    public override int GetHashCode() => HashCode.Combine(Name, ContactItems, AddressItems, CalendarItems);
    public static bool operator ==(InputModel? left, InputModel? right) => EqualityComparer<InputModel>.Default.Equals(left, right);
    public static bool operator !=(InputModel? left, InputModel? right) => !(left == right);
}

public sealed class ContactModel
{
    public string Id { get; init; } = null!;
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public sealed class AddressModel
{
    public string Id { get; init; } = null!;

    [Required]
    public string Label { get; set; } = null!;

    [Display(Name = "Address")]
    public string? Address1 { get; set; }

    [Display(Name = "Address 2")]
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }

    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }
}

public sealed class CalendarModel
{
    public string Id { get; init; } = null!;

    public CalendarRecordType Type { get; set; }
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; }

    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; }
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

    public static ContactModel Clone(this ContactModel subject) => new ContactModel
    {
        Id = subject.Id,
        Type = subject.Type,
        Value = subject.Value,
    };

    public static AddressModel Clone(this AddressModel subject) => new AddressModel
    {
        Id = subject.Id,
        Label = subject.Label,
        Address1 = subject.Address1,
        Address2 = subject.Address2,
        City = subject.City,
        State = subject.State,
        ZipCode = subject.ZipCode,
    };

    public static CalendarModel Clone(this CalendarModel subject) => new CalendarModel
    {
        Id = subject.Id,
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
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

    public static ContactModel ConvertTo(this ContactRecord subject) => new ContactModel
    {
        Id = subject.Id,
        Type = subject.Type.ToString(),
        Value = subject.Value,
    };

    public static ContactRecord ConvertTo(this ContactModel subject) => new ContactRecord
    {
        Id = subject.Id,
        Type = Enum.Parse<ContactType>(subject.Type),
        Value = subject.Value,
    };

    public static AddressModel ConvertTo(this AddressRecord subject) => new AddressModel
    {
        Id = subject.Id,
        Label = subject.Label,
        Address1 = subject.Address1,
        Address2 = subject.Address2,
        City = subject.City,
        State = subject.State,
        ZipCode = subject.ZipCode,
    };

    public static AddressRecord ConvertTo(this AddressModel subject) => new AddressRecord
    {
        Id = subject.Id,
        Label = subject.Label,
        Address1 = subject.Address1,
        Address2 = subject.Address2,
        City = subject.City,
        State = subject.State,
        ZipCode = subject.ZipCode,
    };

    public static CalendarModel ConvertTo(this CalendarRecord subject) => new CalendarModel
    {
        Id = subject.Id,
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };

    public static CalendarRecord ConvertTo(this CalendarModel subject) => new CalendarRecord
    {
        Id = subject.Id,
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };
}
