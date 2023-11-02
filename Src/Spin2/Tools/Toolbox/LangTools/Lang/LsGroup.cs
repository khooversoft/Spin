using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

[DebuggerDisplay("StartToken={StartToken}, EndToken={EndToken}, Name={Name}")]
public class LsGroup : LangBase<ILangSyntax>, ILangRoot
{
    public LsGroup(string startToken, string endToken, string? name)
    {
        StartToken = startToken;
        EndToken = endToken;
        Name = name;
    }

    public string StartToken { get; }
    public string EndToken { get; }
    public string? Name { get; }

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? _)
    {
        var nodes = new LangNodes();

        if (!pContext.TokensCursor.TryPeekValue(out var token)) return (StatusCode.BadRequest, "no tokens");
        if (!(token is TokenValue tokenValue)) return (StatusCode.BadRequest, $"Syntax error: no start token");
        if (tokenValue.Value != StartToken) return (StatusCode.BadRequest, $"Syntax error: not start token={tokenValue.Value}");

        pContext.TokensCursor.NextValue().Assert(x => x.IsOk(), "Failed to get token");
        nodes += new LangNode(this, tokenValue.Value);

        var result = this.MatchSyntaxSegement(nameof(LsGroup), pContext);
        if (result.IsError()) return result;

        nodes += result.Return();

        if (!pContext.TokensCursor.TryNextValue(out var lastToken)) return (StatusCode.BadRequest, "No ending token");
        if (lastToken.Value != EndToken) return (StatusCode.BadRequest, $"No ending token={lastToken.Value}");
        nodes += new LangNode(this, lastToken.Value);

        return nodes;
    }

    public override string ToString() => $"{nameof(LsGroup)}: StartToken={StartToken}, EndToken={EndToken}, Name={Name}";

    public static LsGroup operator +(LsGroup subject, ILangRoot value) => subject.Action(x => x.Children.Add(value));
    public static LsGroup operator +(LsGroup subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsGroup operator +(LsGroup subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsGroup operator +(LsGroup subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}
