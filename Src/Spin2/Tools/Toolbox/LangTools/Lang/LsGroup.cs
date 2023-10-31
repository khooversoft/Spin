using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools;

[DebuggerDisplay("StartToken={StartToken}, EndToken={EndToken}, Name={Name}")]
public class LsGroup : LsRoot, ILangSyntax
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

    public Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax> syntaxCursor)
    {
        bool atLeastOne = false;
        var nodes = new LangNodes();

        while (pContext.TokensCursor.TryPeekValue(out var token))
        {
            if (!(token is TokenValue tokenValue)) return (StatusCode.BadRequest, $"Syntax error: no start token");
            if (tokenValue.Value != StartToken) return (StatusCode.BadRequest, $"Syntax error: not start token={tokenValue.Value}");

            pContext.TokensCursor.NextValue().Assert(x => x.IsOk(), "Failed to get token");
            nodes += new LangNode(this, tokenValue.Value);

            var result = this.MatchSyntaxSegement(pContext);
            if (result.IsError()) break;

            nodes += result.Return();
            atLeastOne = true;

            if (!pContext.TokensCursor.TryNextValue(out var lastToken)) return (StatusCode.BadRequest, "No ending token");
            if (lastToken.Value != EndToken) return (StatusCode.BadRequest, $"No ending token={lastToken.Value}");
            nodes += new LangNode(this, lastToken.Value);
        }

        if (!atLeastOne) return (StatusCode.BadRequest, "Syntax error");

        return nodes;
    }

    public static LsGroup operator +(LsGroup subject, ILangSyntax value) => subject.Action(x => x.Children.Add(value));
    public static LsGroup operator +(LsGroup subject, string symbol) => subject.Action(x => x.Children.Add(new LsToken(symbol)));

    public static LsGroup operator +(LsGroup subject, (string symbol, string name) data)
    {
        subject.Children.Add(new LsToken(data.symbol, data.name));
        return subject;
    }
}
