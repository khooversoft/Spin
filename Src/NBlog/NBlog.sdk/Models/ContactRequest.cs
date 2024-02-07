using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.Models;

[GenerateSerializer, Immutable]
public record ContactRequest
{
    [Id(0)] public string Name { get; init; } = null!;
    [Id(1)] public string Email { get; init; } = null!;
    [Id(2)] public string Message { get; init; } = null!;

    public static IValidator<ContactRequest> Validator { get; } = new Validator<ContactRequest>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Email).ValidEmail()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}


public static class ContactRequestExtensions
{
    public static Option Validate(this ContactRequest subject) => ContactRequest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ContactRequest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
