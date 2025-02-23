using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public record TicketMasterOption
{
    public string ApiKey { get; init; } = null!;
    public string DiscoveryUrl { get; init; } = null!;

    public IValidator<TicketMasterOption> Validator => new Validator<TicketMasterOption>()
        .RuleFor(x => x.ApiKey).NotEmpty()
        .RuleFor(x => x.DiscoveryUrl).NotEmpty()
        .Build();
}

public static class TicketMasterOptionExtensions
{
    public static Option Validate(this TicketMasterOption subject) => subject.Validator.Validate(subject).ToOptionStatus();
}
