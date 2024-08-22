using System.Collections.Frozen;
using Toolbox.Types;

namespace Toolbox.LangTools;

internal static class MetaSyntaxTool
{
    public readonly struct GroupToken
    {
        public ProductionRuleType Type { get; init; }
        public string CloseSymbol { get; init; }
        public string Label { get; init; }
    }

    public static readonly FrozenDictionary<string, GroupToken> GroupTokens = new Dictionary<string, GroupToken>
    {
        ["("] = new GroupToken { Type = ProductionRuleType.Or, CloseSymbol = ")", Label = "OrGroup" },
        ["["] = new GroupToken { Type = ProductionRuleType.Optional, CloseSymbol = "]", Label = "OptionGroup" },
        ["{"] = new GroupToken { Type = ProductionRuleType.Repeat, CloseSymbol = "}", Label = "RepeatGroup" },
    }.ToFrozenDictionary();

    public static readonly FrozenSet<string> ParseTokens = new string[]
    {
        "=", ",", ";", "|", "[", "]", "{", "}", "(", ")"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool TryGetGroupToken(string token, out GroupToken groupToken) => GroupTokens.TryGetValue(token, out groupToken);

    public static string ErrorMessage(this MetaParserContext parserContext, string message) =>
        $"Error: {message} at '{parserContext.TokensCursor.Current?.Index ?? -1}', token='{(parserContext.TokensCursor.DebugCursorLocation())}'";
}