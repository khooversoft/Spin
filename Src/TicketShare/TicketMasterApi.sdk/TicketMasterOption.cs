using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public record TicketMasterOption
{
    public string ApiKey { get; init; } = null!;
    public string EventUrl { get; init; } = null!;
    public string ClassificationUrl { get; init; } = null!;
    public bool UseCache { get; init; }

    public IValidator<TicketMasterOption> Validator => new Validator<TicketMasterOption>()
        .RuleFor(x => x.ApiKey).NotEmpty()
        .RuleFor(x => x.EventUrl).NotEmpty()
        .RuleFor(x => x.ClassificationUrl).NotEmpty()
        .Build();
}

public static class TicketMasterOptionExtensions
{
    public static Option Validate(this TicketMasterOption subject) => subject.Validator.Validate(subject).ToOptionStatus();
}
