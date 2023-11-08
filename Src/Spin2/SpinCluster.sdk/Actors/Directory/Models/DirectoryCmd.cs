using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Directory;

[GenerateSerializer, Immutable]
public sealed record DirectoryCmd
{
    public DirectoryCmd() { }
    public DirectoryCmd(string command) => Commands = new[] { command };
    public DirectoryCmd(IEnumerable<string> commands) => Commands = commands.ToArray();

    [Id(0)] public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();

    public override string ToString() => Commands.NotNull().Join();

    public static IValidator<DirectoryCmd> Validator { get; } = new Validator<DirectoryCmd>()
        .RuleFor(x => x.Commands).NotNull()
        .RuleForEach(x => x.Commands).NotEmpty()
        .Build();
}


public static class DirectoryCmdExtensions
{
    public static Option Validate(this DirectoryCmd subject) => DirectoryCmd.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryCmd subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
