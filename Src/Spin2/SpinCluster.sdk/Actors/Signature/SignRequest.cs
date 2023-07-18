using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Signature;

public record SignRequest
{
    public string Digest { get; init; } = null!;
}

public static class SignRequestValidator
{
    public static IValidator<SignRequest> Validator { get; } = new Validator<SignRequest>()
        .RuleFor(x => x.Digest).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this SignRequest signRequest, ScopeContextLocation location) => Validator
        .Validate(signRequest)
        .LogResult(location);
}
