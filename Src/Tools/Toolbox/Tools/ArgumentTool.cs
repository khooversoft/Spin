using Toolbox.Extensions;

namespace Toolbox.Tools;

public class ArgumentTool
{
    public static (string[] ConfigArgs, string[] CommandLineArgs) Split(string[] args) => args.NotNull()
        .Select(x => (config: x.Split('=').Length > 1 ? 0 : 1, arg: x))
        .Func(x => (
            configArgs: x.Where(y => y.config == 0).Select(x => x.arg).ToArray(),
            cmdArgs: x.Where(y => y.config == 1).Select(x => x.arg).ToArray())
            );
}
