using System.Collections;
using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Toolbox.Extensions;

namespace Toolbox.Tools.Dump;

public static class AppEnvironment
{
    public static IReadOnlyList<string> Dump(string[] args, IConfiguration configuration, bool includeEnvironmentVariables = false)
    {
        args.NotNull();
        configuration.NotNull();

        var list = new[]
        {
            ["Args..."],
            args.WithIndex().Select(x => $"Arg: {x.Index}='{x.Item}'"),
            ["Env..."],
            Environment.GetEnvironmentVariables()
                .OfType<DictionaryEntry>()
                .Where(_ => includeEnvironmentVariables)
                .OrderBy(x => x.Key)
                .Select(x => $"Env: {x.Key}='{x.Value}'"),

            configuration.AsEnumerable().Select(x => $"Config: {x.Key}='{x.Value}'"),
        }
        .SelectMany(x => x)
        .ToImmutableArray();

        return list;
    }
}
