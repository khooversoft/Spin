using TicketApi.sdk.Model;
using Toolbox.Tools;

namespace TicketApi.sdk;

public sealed record AttractionRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public string? Locale { get; init; }

    public bool Equals(AttractionRecord? other)
    {
        var result = other is AttractionRecord subject &&
            Id == subject.Id &&
            Name == subject.Name &&
            Url == subject.Url &&
            Locale == subject.Locale;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Id, Name, Url, Locale);

    public static IValidator<AttractionRecord> Validator { get; } = new Validator<AttractionRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();
}


public static class AttractionRecordExtensions
{
    public static AttractionRecord ConvertTo(this AttractionModel subject) => new AttractionRecord
    {
        Id = subject.Id,
        Name = subject.Name,
        Url = subject.Url,
        Locale = subject.Locale,
    };
}
