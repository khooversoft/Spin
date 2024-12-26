using TicketShare.sdk;
using Toolbox.Extensions;

namespace TicketShareWeb.Components.Pages.Profile.Models;

public sealed record AddressModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string Label { get; set; } = null!;
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }

    public bool HasAddress => Address1.IsNotEmpty() ||
        Address2.IsNotEmpty() ||
        City.IsNotEmpty() ||
        State.IsNotEmpty() ||
        ZipCode.IsNotEmpty();

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
        .Select(x => x.ToNullIfEmpty())
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