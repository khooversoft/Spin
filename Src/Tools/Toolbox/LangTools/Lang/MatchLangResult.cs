//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.LangTools;

//public interface IMatchLangResult
//{
//    Option IsMatch(LangNode node);
//}

//public record MatchLangResult<T> : IMatchLangResult
//{
//    public MatchLangResult(string value, string? name = null)
//    {
//        Value = value;
//        Name = name;
//    }
//    public string Value { get; init; } = null!;
//    public string? Name { get; }

//    public Option IsMatch(LangNode node)
//    {
//        if (node.SyntaxNode is not T) return (StatusCode.BadRequest, $"SyntaxNode={node.SyntaxNode.GetType().Name}, T={typeof(T).Name}");
//        if (Name.IsNotEmpty() && node.SyntaxNode.Name != Name) return (StatusCode.BadRequest, $"Node name={node.SyntaxNode.Name} does not match name={Name}");
//        if (node.Value != Value) return (StatusCode.BadRequest, $"Value={node.Value} does not match value={Value}");

//        return StatusCode.OK;
//    }
//}
