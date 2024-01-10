using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class ArticleManifestExtensions
{
    public static Option Validate(this ArticleManifest subject) => ArticleManifest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ArticleManifest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ArticleManifest Verify(this ArticleManifest subject) => subject.Action(x => x.Validate().ThrowOnError());

    public static IReadOnlyList<CommandNode> GetCommands(this ArticleManifest subject) => subject.Commands.SelectMany(x => CommandGrammarParser.Parse(x).Return()).ToArray();

    public static Option<CommandNode> GetCommand(this ArticleManifest subject, string attribute) => subject.GetCommands()
        .Where(x => x.Attributes.Any(y => y == attribute))
        .FirstOrDefaultOption();
}
