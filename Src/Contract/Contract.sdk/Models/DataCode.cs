using Toolbox.Tools;

namespace Contract.sdk.Models;

public record DataCode
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; }
    public string Language { get; init; } = "C#";
    public string Framework { get; init; } = ".net7.0";
    public IReadOnlyList<string> Lines { get; init; } = Array.Empty<string>();
}

public static class ContractBlkCodeExtensions
{
    public static void Verify(this DataCode dataCode)
    {
        dataCode.NotNull();
        dataCode.Language.NotEmpty();
        dataCode.Framework.NotEmpty();

        dataCode.Lines
            .NotNull()
            .Assert(x => x.Count > 0, $"{dataCode.Lines} is empty");
    }
}
