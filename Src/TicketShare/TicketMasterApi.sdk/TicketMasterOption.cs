using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public record TicketMasterOption
{
    public string ApiKey { get; init; } = null!;
    public string DiscoveryUrl { get; init; } = null!;
    public IReadOnlyList<TicketMasterSearch> Searches { get; init; } = Array.Empty<TicketMasterSearch>();

    public IValidator<TicketMasterOption> Validator => new Validator<TicketMasterOption>()
        .RuleFor(x => x.ApiKey).NotEmpty()
        .RuleFor(x => x.DiscoveryUrl).NotEmpty()
        .RuleFor(x => x.Searches).NotNull().Must(x => x.Count > 0, _ => "At least one search is required")
        .RuleForEach(x => x.Searches).Validate(TicketMasterSearch.Validator)
        .Build();
}

public static class TicketMasterOptionExtensions
{
    public static Option Validate(this TicketMasterOption subject) => subject.Validator.Validate(subject).ToOptionStatus();
}
