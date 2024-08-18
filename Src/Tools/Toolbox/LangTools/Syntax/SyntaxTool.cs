namespace Toolbox.LangTools;

internal static class SyntaxTool
{
    public static string ErrorMessage(this SyntaxParserContext parserContext, string message) =>
        $"Error: {message} at '{parserContext.TokensCursor.Current.Index}', token='{parserContext.TokensCursor.Current.Value}'";
}
