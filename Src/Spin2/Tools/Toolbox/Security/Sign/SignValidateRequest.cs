using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Security.Sign;

public record SignValidateRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}

public static class SignValidateRequestValidator
{
    public static IValidator<SignValidateRequest> Validator { get; } = new Validator<SignValidateRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();

    public static Option Validate(this SignValidateRequest request) => Validator.Validate(request).ToOptionStatus();
}
