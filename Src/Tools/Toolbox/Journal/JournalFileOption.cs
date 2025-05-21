using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Journal;

public class JournalFileOption
{
    // Connection string format: "journal=/journal/data";
    public string ConnectionString { get; init; } = null!;

    // Writes are performed in the background
    public bool UseBackgroundWriter { get; init; }

    public static IValidator<JournalFileOption> Validator { get; } = new Validator<JournalFileOption>()
        .RuleFor(x => x.ConnectionString).NotEmpty()
        .Build();
}

public static class JournalFileOptionExtensions
{
    public static Option Validate(this JournalFileOption option) => JournalFileOption.Validator.Validate(option).ToOptionStatus();
}
