using System.ComponentModel.DataAnnotations;
using TicketShare.sdk;
using Toolbox.Extensions;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record AddressModel
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

    public override string ToString()
    {
        string v = new string?[]
        {
            Label,
            Address1,
            Address2,
            City,
            State,
            ZipCode,
        }
        .OfType<string>()
        .Join(", ");

        return v;
    }
}


public static class AddressModelExtensions
{
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
}