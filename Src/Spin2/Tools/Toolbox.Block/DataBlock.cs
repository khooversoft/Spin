using Toolbox.Security.Jwt;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public record DataBlock
{
    public string BlockId { get; init; } = Guid.NewGuid().ToString();
    public long TimeStamp { get; init; } = UnixDate.UtcNow;

    // Stream name or name of the block type
    public string BlockType { get; init; } = null!;

    // Object that is serialized
    public string ClassType { get; init; } = null!;

    // Json data
    public string Data { get; init; } = null!;

    // Ower of the data
    public string PrincipleId { get; init; } = null!;

    // Signed Digist
    public string JwtSignature { get; init; } = null!;

    // Hash of data block except JwtSignature
    public string Digest { get; init; } = null!;
}


public static class DataBlockValidator
{
    public static IValidator<DataBlock> Validator { get; } = new Validator<DataBlock>()
        .RuleFor(x => x.BlockId).NotEmpty()
        .RuleFor(x => x.TimeStamp).Must(x => x > 0, _ => "Invalid timestamp")
        .RuleFor(x => x.BlockType).NotEmpty()
        .RuleFor(x => x.ClassType).NotNull()
        .RuleFor(x => x.Data).NotNull()
        .RuleFor(x => x.PrincipleId).NotEmpty()
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .RuleFor(x => x.Digest).NotEmpty()
        .RuleForObject(x => x).Must(x => x.CalculateDigest() == x.Digest, _ => "Digest doest not match")
        .Build();

    public static void Verify(this DataBlock subject) => Validator.Validate(subject).ThrowOnError();

    public static ValidatorResult Validate(this DataBlock subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static async Task<Option<JwtTokenDetails>> ValidateDigest(this DataBlock subject, ISignValidate signValidate, ScopeContext context)
    {
        var valResult = Validator.Validate(subject).LogResult(context.Location()).ToOption<ValidatorResult>();
        if (valResult.IsError()) return valResult.ToOption<JwtTokenDetails>();

        return await signValidate.ValidateDigest(subject.JwtSignature, subject.Digest, context);
    }
}