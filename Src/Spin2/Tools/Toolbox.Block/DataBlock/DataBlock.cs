using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public record DataBlock
{
    public string BlockId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

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

    public static IValidator<DataBlock> Validator { get; } = new Validator<DataBlock>()
        .RuleFor(x => x.BlockId).NotEmpty()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.BlockType).ValidName()
        .RuleFor(x => x.ClassType).NotNull()
        .RuleFor(x => x.Data).NotNull()
        .RuleFor(x => x.PrincipleId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.JwtSignature).NotEmpty()
        .RuleFor(x => x.Digest).NotEmpty()
        .RuleForObject(x => x).Must(x => x.CalculateDigest() == x.Digest, _ => "Digest validation failed")
        .Build();
}


public static class DataBlockValidator
{

    public static void Verify(this DataBlock subject) => DataBlock.Validator.Validate(subject).ThrowOnError();

    public static Option Validate(this DataBlock subject) => DataBlock.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DataBlock subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static async Task<Option> ValidateDigest(this DataBlock subject, ISignValidate signValidate, ScopeContext context)
    {
        var valResult = DataBlock.Validator.Validate(subject);
        if (valResult.IsError()) return valResult.ToOptionStatus();

        return await signValidate.ValidateDigest(subject.JwtSignature, subject.Digest, context.TraceId);
    }
}