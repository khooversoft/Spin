using Toolbox.Tools;

namespace TicketApi.sdk.Model;

public sealed record AttractionModel
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Url { get; init; }
    public string? Locale { get; init; }
    public IReadOnlyList<ImageModel> Images { get; init; } = Array.Empty<ImageModel>();

    public bool Equals(AttractionModel? other) =>
        other != null &&
        Id == other.Id &&
        Name == other.Name &&
        Url == other.Url &&
        Locale == other.Locale &&
        Images.OrderBy(x => x.Url).SequenceEqual(other.Images);

    public override int GetHashCode() => HashCode.Combine(Id, Name, Url, Locale, Images);

    public static IValidator<AttractionModel> Validator { get; } = new Validator<AttractionModel>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.Images).Validate(ImageModel.Validator)
        .Build();
}
