using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk.MasterList;

public record TeamDetail
{
    public string League { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IReadOnlyList<TeamClassification> Segments { get; init; } = Array.Empty<TeamClassification>();
    public IReadOnlyList<TeamClassification> Genres { get; init; } = Array.Empty<TeamClassification>();
    public IReadOnlyList<TeamClassification> SubGenres { get; init; } = Array.Empty<TeamClassification>();

    public static IValidator<TeamDetail> Validator => new Validator<TeamDetail>()
        .RuleFor(x => x.League).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.Segments).Validate(TeamClassification.Validator)
        .RuleForEach(x => x.Genres).Validate(TeamClassification.Validator)
        .RuleForEach(x => x.SubGenres).Validate(TeamClassification.Validator)
        .Build();
}

public record TeamClassification
{
    public string Attribute { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Id { get; init; } = null!;

    public static IValidator<TeamClassification> Validator => new Validator<TeamClassification>()
        .RuleFor(x => x.Attribute).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Id).NotEmpty()
        .Build();
}

public static class TeamDetailExtensions
{
    public static Option Validate(this TeamDetail subject) => TeamDetail.Validator.Validate(subject).ToOptionStatus();
}