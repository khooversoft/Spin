using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public record TeamDetail
{
    public string League { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string LeagueName { get; init; } = null!;
    public TeamClassification Segment { get; init; } = null!;
    public TeamClassification Genre { get; init; } = null!;
    public TeamClassification SubGenre { get; init; } = null!;
    public IReadOnlyList<string> Venues { get; init; } = Array.Empty<string>();

    public static IValidator<TeamDetail> Validator => new Validator<TeamDetail>()
        .RuleFor(x => x.League).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.LeagueName).NotEmpty()
        .RuleFor(x => x.Segment).Validate(TeamClassification.Validator)
        .RuleFor(x => x.Genre).Validate(TeamClassification.Validator)
        .RuleFor(x => x.SubGenre).Validate(TeamClassification.Validator)
        .RuleFor(x => x.Venues).NotNull().Must(x => x.Count > 0, _ => "Venues cannot be empty")
        .RuleForEach(x => x.Venues).NotEmpty()
        .Build();
}

public record TeamClassification
{
    public string Name { get; init; } = null!;
    public string Id { get; init; } = null!;

    public static IValidator<TeamClassification> Validator => new Validator<TeamClassification>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Id).NotEmpty()
        .Build();
}

public static class TeamDetail2Extensions
{
    public static Option Validate(this TeamDetail subject) => TeamDetail.Validator.Validate(subject).ToOptionStatus();
}