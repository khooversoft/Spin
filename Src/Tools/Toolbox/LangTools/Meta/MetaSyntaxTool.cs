using System.Collections.Frozen;

namespace Toolbox.LangTools;

public static class MetaSyntaxTool
{
    public readonly struct GroupToken
    {
        public ProductionRuleType Type { get; init; }
        public string CloseSymbol { get; init; }
    }

    public static readonly FrozenDictionary<string, GroupToken> GroupTokens = new Dictionary<string, GroupToken>
    {
        ["("] = new GroupToken { Type = ProductionRuleType.Group, CloseSymbol = ")" },
        ["["] = new GroupToken { Type = ProductionRuleType.Optional, CloseSymbol = "]" },
        ["{"] = new GroupToken { Type = ProductionRuleType.Repeat, CloseSymbol = "}" },
    }.ToFrozenDictionary();

    public static readonly FrozenSet<string> ParseTokens = new string[]
    {
        "=", ",", ";", "|", "[", "]", "{", "}", "(", ")"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool TryGetGroupToken(string token, out GroupToken groupToken) => GroupTokens.TryGetValue(token, out groupToken);
}