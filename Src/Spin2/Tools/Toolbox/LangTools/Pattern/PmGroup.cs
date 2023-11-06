using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.LangTools.Pattern;

public class PmGroup : PatternBase
{
    public PmGroup(string startToken, string endToken, string? name)
        : base(name)
    {
        StartToken = startToken;
        EndToken = endToken;
    }

    public string StartToken { get; }
    public string EndToken { get; }

    public override Option<Sequence<IPatternSyntax>> Process(PatternContext pContext)
    {
        var nodes = new Sequence<IPatternSyntax>();

        if (!pContext.TokensCursor.TryPeekValue(out var token)) return (StatusCode.BadRequest, "no tokens");
        if (!(token is TokenValue tokenValue)) return (StatusCode.BadRequest, $"Syntax error: no start token");
        if (tokenValue.Value != StartToken) return (StatusCode.BadRequest, $"Syntax error: not start token={tokenValue.Value}");

        pContext.TokensCursor.NextValue().Assert(x => x.IsOk(), "Failed to get token");
        nodes += (IPatternSyntax)new PmToken(StartToken, name: $"{Name}-start");

        var result = this.MatchSyntaxSegement(pContext);
        if (result.IsError()) return result;

        nodes += result.Return();

        if (!pContext.TokensCursor.TryNextValue(out var lastToken)) return (StatusCode.BadRequest, "No ending token");
        if (lastToken.Value != EndToken) return (StatusCode.BadRequest, $"No ending token={lastToken.Value}");
        nodes += (IPatternSyntax)new PmToken(StartToken, name: $"{Name}-end");

        return nodes;
    }
}