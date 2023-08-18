using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Security.Sign;

public record ValidateRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipleId { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}

public static class ValidateRequestValidator
{
    public static IValidator<ValidateRequest> Validator { get; } = new Validator<ValidateRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipleId).NotEmpty()
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();
}
