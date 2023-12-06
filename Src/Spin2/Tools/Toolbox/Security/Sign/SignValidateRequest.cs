using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security.Sign;

public record SignValidateRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;

    public static IValidator<SignValidateRequest> Validator { get; } = new Validator<SignValidateRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();
}

public static class SignValidateRequestValidator
{
    public static Option Validate(this SignValidateRequest subject) => SignValidateRequest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SignValidateRequest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
