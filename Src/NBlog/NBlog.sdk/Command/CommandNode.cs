using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record CommandNode
{
    public IReadOnlyList<string> Attributes { get; init; } = Array.Empty<string>();
    public string FileId { get; init; } = null!;
    public string FileIdValue { get; init; } = null!;

    public bool IsFileReference => Attributes.Any(x => NBlogConstants.FileAttributes.Contains(x));
    public bool IsIndexReference => Attributes.Any(x => NBlogConstants.IndexAttributes.Contains(x));
    public bool IsSearchReference => Attributes.Any(x => NBlogConstants.SearchAttributes.Contains(x));

    public static IValidator<CommandNode> Validator { get; } = new Validator<CommandNode>()
        .RuleForEach(x => x.Attributes).ValidName()
        .RuleFor(x => x.FileId).Must(x => NBlog.sdk.FileId.Create(x).IsOk(), x => $"FileId={x} is invalid")
        .RuleFor(x => x.FileIdValue).NotEmpty()
        .RuleForObject(x => x).Must(x => x.IsAttributesValid(), x => $"{x} is invalid attribute")
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

    public static bool IsAttributesValid(this CommandNode subject)
    {
        int[] map = new[]
        {
            subject.Attributes.Contains(NBlogConstants.SummaryAttribute) ? 1 : 0,
            subject.Attributes.Contains(NBlogConstants.MainAttribute) ? 1 : 0,
            subject.Attributes.Contains(NBlogConstants.ImageAttribute) ? 10 : 0,
            subject.Attributes.Contains(NBlogConstants.IndexAttribute) ? 20 : 0,
        };

        bool isValid = map.Sum() switch
        {
            1 or 2 => true,
            10 => true,
            20 => true,
            _ => false,
        };

        return isValid;
    }
}
