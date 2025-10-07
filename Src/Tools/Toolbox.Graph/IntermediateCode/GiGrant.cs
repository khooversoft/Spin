using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum GrantCommand
{
    None,
    Grant,
    Revoke
}

public enum GrantType
{
    None,
    Reader,
    Contributor,
    Owner,
}

internal record GiGrant : IGraphInstruction
{
    public GrantCommand GrantCommand { get; init; }
    public GrantType GrantType { get; init; }
    public string PrincipalIdentifier { get; init; } = null!;
    public string NameIdentifier { get; init; } = null!;
}

internal static class GiGrantTool
{
    /// example: grant {grantType} to {principal} on {nameIdentifier}
    /// grant types = Reader, Contributor, Owner
    public static Option<IGraphInstruction> Build(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var cmd = ic.GetEnum<GrantCommand>("grant-sym", "revoke-sym");
        if (cmd.IsError()) return cmd.ToOptionStatus<IGraphInstruction>();

        var grantType = ic.GetEnum<GrantType>("reader-sym", "contributor-sym", "owner-sym");
        if (grantType.IsError()) return grantType.ToOptionStatus<IGraphInstruction>();

        var s2 = ic.ProcessSymbols(["to-sym"]);
        if (s2.IsError()) return s2.ToOptionStatus<IGraphInstruction>();

        var principal = ic.GetValue("principalIdentifier");
        if (principal.IsError()) return principal.ToOptionStatus<IGraphInstruction>();

        var s3 = ic.ProcessSymbols(["on-sym"]);
        if (s3.IsError()) return s3.ToOptionStatus<IGraphInstruction>();

        var nameIdentifier = ic.GetValue("nameIdentifier");
        if (nameIdentifier.IsError()) return nameIdentifier.ToOptionStatus<IGraphInstruction>();

        var term = ic.GetValue("term");
        if (term.IsError()) return term.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiGrant()
        {
            GrantCommand = cmd.Return(),
            GrantType = grantType.Return(),
            PrincipalIdentifier = principal.Return(),
            NameIdentifier = nameIdentifier.Return(),
        };
    }

    public static string GetCommandDesc(this GiGrant subject)
    {
        var command = new[]
        {
            nameof(GiGrant),
            $"GrantType={subject.GrantType}",
            $"PrincipalIdentifier={subject.PrincipalIdentifier}",
            $"NameIdentifier={subject.NameIdentifier}"
        }.Join(", ");

        return command;
    }
}