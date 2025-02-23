namespace TicketMasterApi.sdk;

public record VenueModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? PostalCode { get; init; }
    public string? TimeZone { get; init; }
    public StateModel? State { get; init; }
    public CityModel? City { get; init; }
    public CountryModel? Country { get; init; }
    public AddressModel? Address { get; init; }
}

public record CityModel
{
    public string? Name { get; init; }
}

public record StateModel
{
    public string? Name { get; init; }
    public string? StateCode { get; init; }
}

public record CountryModel
{
    public string? Name { get; init; }
    public string? CountryCode { get; init; }
}

public record AddressModel
{
    public string? Line1 { get; init; }
}
