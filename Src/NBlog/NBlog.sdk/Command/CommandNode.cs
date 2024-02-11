using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record CommandNode
{
    public IReadOnlyList<string> Attributes { get; init; } = Array.Empty<string>();
    public string FileId { get; init; } = null!;
    public string LocalFilePath { get; init; } = null!;

    public bool IsFileReference() => Attributes.Count(x => NBlogConstants.FileAttributes.Contains(x)) == Attributes.Count;
    public bool IsIndexReference() => Attributes.Count(x => NBlogConstants.IndexAttributes.Contains(x)) == Attributes.Count;

    public static IValidator<CommandNode> Validator { get; } = new Validator<CommandNode>()
        .RuleForEach(x => x.Attributes).ValidName()
        .RuleFor(x => x.FileId).Must(x => NBlog.sdk.FileId.Create(x).IsOk(), x => $"FileId={x} is invalid")
        .RuleFor(x => x.LocalFilePath).NotEmpty()
        .Build();
}

public static class CommandNodeExtensions
{
    public static Option Validate(this CommandNode subject) => CommandNode.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this CommandNode subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
