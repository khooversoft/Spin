using TicketApi.sdk.Model;
using Toolbox.Tools;

namespace TicketApi.sdk;

public record VenueRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? TimeZone { get; init; }

    public static IValidator<VenueRecord> Validator { get; } = new Validator<VenueRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();
}

public static class VenueRecordExtensions
{
    public static VenueRecord ConvertTo(this Event_VenueModel subject) => new VenueRecord
    {
        Id = subject.Id,
        Name = subject.Name,
        Address = subject.Address?.Line1,
        City = subject.City?.Name,
        State = subject.State?.Name,
        PostalCode = subject.PostalCode,
        Country = subject.Country?.Name,
        TimeZone = subject.TimeZone,
    };
}
