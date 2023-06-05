using System.Text.Json.Nodes;
using FluentValidation;
using Toolbox.Extensions;
using Toolbox.Tools;
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

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(x => x.ObjectId).NotEmpty()
            .Must(x => ObjectId.IsObjectIdValid(x).IsOk())
            .WithMessage($"Must match {ObjectId.Syntax}");

        RuleFor(x => x.TypeName).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();

        RuleFor(x => x.ETag)
            .Must((x, p) => x.IsHashVerify())
            .When(x => x.ETag.IsNotEmpty())
            .WithMessage("ETag does not match");
    }

    public static DocumentValidator Default { get; } = new DocumentValidator();
}
