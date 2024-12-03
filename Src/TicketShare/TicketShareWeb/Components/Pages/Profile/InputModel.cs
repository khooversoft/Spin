using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;
using Toolbox.Tools;

namespace TicketShareWeb.Components.Pages.Profile;

public sealed class InputModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    public IReadOnlyList<ContactModel> ContactItems { get; init; } = new List<ContactModel>();
    public IReadOnlyList<AddressModel> AddressItems { get; init; } = new List<AddressModel>();
    public IReadOnlyList<CalendarModel> CalendarItems { get; init; } = new List<CalendarModel>();
}

public sealed class ContactModel
{
    public string Type { get; set; } = null!;
    public string Value { get; set; } = null!;
}

public sealed class AddressModel
{
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
    public CalendarRecordType Type { get; set; }
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; }

    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; }
}

public static class InputModelExtensions
{
    public static AccountRecord ConvertTo(this InputModel subject, string principalId)
    {
        subject.NotNull();

        return new AccountRecord
        {
            PrincipalId = principalId.NotEmpty(),
            Name = subject.Name,
            ContactItems = subject.ContactItems?.Select(x => x.ConvertTo())?.ToImmutableArray() ?? [],
            AddressItems = subject.AddressItems?.Select(x => x.ConvertTo())?.ToImmutableArray() ?? [],
            CalendarItems = subject.CalendarItems?.Select(x => x.ConvertTo()).ToImmutableArray() ?? [],
        };
    }

    public static InputModel ConvertTo(this AccountRecord subject)
    {
        subject.NotNull();
        return new InputModel
        {
            Name = subject.Name.NotEmpty(),
            ContactItems = subject.ContactItems.Select(x => x.ConvertTo()).ToList(),
            AddressItems = subject.AddressItems.Select(x => x.ConvertTo()).ToList(),
            CalendarItems = subject.CalendarItems.Select(x => x.ConvertTo()).ToList(),
        };
    }

    public static ContactModel ConvertTo(this ContactRecord subject) => new ContactModel
    {
        Type = subject.Type.ToString(),
        Value = subject.Value,
    };

    public static ContactRecord ConvertTo(this ContactModel subject) => new ContactRecord
    {
        Type = Enum.Parse<ContactType>(subject.Type),
        Value = subject.Value,
    };

    public static AddressModel ConvertTo(this AddressRecord subject) => new AddressModel
    {
        Label = subject.Label,
        Address1 = subject.Address1,
        Address2 = subject.Address2,
        City = subject.City,
        State = subject.State,
        ZipCode = subject.ZipCode,
    };

    public static AddressRecord ConvertTo(this AddressModel subject) => new AddressRecord
    {
        Label = subject.Label,
        Address1 = subject.Address1,
        Address2 = subject.Address2,
        City = subject.City,
        State = subject.State,
        ZipCode = subject.ZipCode,
    };

    public static CalendarModel ConvertTo(this CalendarRecord subject) => new CalendarModel
    {
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };

    public static CalendarRecord ConvertTo(this CalendarModel subject) => new CalendarRecord
    {
        Type = subject.Type,
        FromDate = subject.FromDate,
        ToDate = subject.ToDate,
    };
}
