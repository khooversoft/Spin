using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record DirectoryCommand
{
    public DirectoryCommand() { }
    public DirectoryCommand(string graphCommand) => Command = graphCommand;

    [Id(0)] public string Command { get; init; } = null!;

    public static IValidator<DirectoryCommand> Validator { get; } = new Validator<DirectoryCommand>()
        .RuleFor(x => x.Command).NotEmpty()
        .Build();
}


public static class DirectoryQueryExtensions
{
    public static Option Validate(this DirectoryCommand subject) => DirectoryCommand.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DirectoryCommand subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
