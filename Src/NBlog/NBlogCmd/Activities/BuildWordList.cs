using NBlog.sdk;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlogCmd.Activities;

public class BuildWordList : ICommandRoute
{
    private readonly BuildWordTokenList _packageBuild;
    public BuildWordList(BuildWordTokenList packageBuild) => _packageBuild = packageBuild.NotNull();

    public CommandSymbol CommandSymbol() => new CommandSymbol("build-word", "Build word token list").Action(x =>
    {
        var basePath = x.AddArgument<string>("basePath", "Path for the base folder to update");
        var wordTokenListFile = x.AddArgument<string>("wordTokenListFile", "Word toke list file to write to");

        x.SetHandler(BuildPackage, basePath, wordTokenListFile);
    });

    private async Task BuildPackage(string basePath, string packageFile)
    {
        await _packageBuild.Build(basePath, packageFile);
    }
}
