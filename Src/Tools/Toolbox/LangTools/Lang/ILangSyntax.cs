//using Toolbox.Types;

//namespace Toolbox.LangTools;

//public interface ILangSyntax
//{
//    string? Name { get; }
//    Option<LangNodes> Process(LangParserContext pContext, Cursor<ILangSyntax>? syntaxCursor = null);

//}


//public static class LangSyntaxExtensions
//{
//    public static Cursor<ILangSyntax> CreateCursor(this ILangSyntax subject)
//    {
//        var cursor = new Cursor<ILangSyntax>(new[] { subject });
//        cursor.NextValue();
//        return cursor;
//    }
//}
