using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Security.Sign;

public sealed record SignResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string PrincipleId { get; init; } = null!;
    public string Kid { get; init; } = null!;
    public string MessageDigest { get; init; } = null!;
    public string JwtSignature { get; init; } = null!;
}


public static class SignResponseValidator
{
    public static IValidator<SignResponse> Validator { get; } = new Validator<SignResponse>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipleId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Kid).ValidResourceId(ResourceType.Owned, "kid")
        .RuleFor(x => x.MessageDigest).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .Build();

    public static Option Validate(this SignResponse request) => Validator.Validate(request).ToOptionStatus();
}