using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Tools.Validation.Validators;
using Toolbox.Types;

namespace Toolbox.DocumentContainer;

public sealed record Document
{
    public required string ObjectId { get; init; } = null!;
    public required string TypeName { get; init; } = null!;
    public required string Content { get; init; } = null!;
    public string? ETag { get; init; }
    public string? Tags { get; init; } = null!;

    public bool Equals(Document? obj)
    {
        return obj is Document document &&
               ObjectId == document.ObjectId &&
               TypeName == document.TypeName &&
               Content == document.Content &&
               ETag == document.ETag &&
               Tags == document.Tags;
    }

    public override int GetHashCode() => HashCode.Combine(ObjectId, TypeName, Content, ETag, Tags);
}

public static class DocumentValidationExtensions
{
    public static Validator<Document> _validator { get; } = new Validator<Document>()
        .RuleFor(x => x.ObjectId).NotEmpty().Must(x => ObjectId.IsValid(x), _ => $"not a valid ObjectId, syntax={ObjectId.Syntax}")
        .RuleFor(x => x.TypeName).NotEmpty()
        .RuleFor(x => x.Content).NotEmpty()
        .RuleFor(x => x.ETag).NotNull()
        .Build();

    public static ValidatorResult Validate(this Document subject) => _validator.Validate(subject);

    public static Document Verify(this Document subject) => subject.Action(x => x.Validate().ThrowOnError());
}