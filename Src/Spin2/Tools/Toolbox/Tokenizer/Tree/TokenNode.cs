//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace Toolbox.Tokenizer.Tree;

//public enum TokenNodeType
//{
//    None,
//    Root,
//    Literal,
//    Symbol,
//    Value
//}

//public class TokenNode : IEnumerable<TokenNode>
//{
//    private readonly List<TokenNode> _children = new List<TokenNode>();
//    private readonly Cursor<TokenNode>? _cursor = null;

//    public TokenNode() => Type = TokenNodeType.Root;

//    public TokenNode(TokenNodeType type, string? name = null, string? token = null)
//    {
//        Type = type;
//        Name = name;
//        Symbol = token;
//    }

//    public TokenNode(TokenNode parent, TokenNodeType type, string? name = null, string? symbol = null, string? value = null)
//    {
//        Parent = parent.NotNull();
//        Type = type;
//        Name = name;
//        Symbol = symbol;
//        Value = value;
//    }

//    public TokenNode Parent { get; init; } = null!;
//    public TokenNodeType Type { get; init; }
//    public string? Name { get; init; }
//    public string? Symbol { get; init; }
//    public string? Value { get; init; }

//    public IList<TokenNode> Children => _children;
//    public Cursor<TokenNode> Cursor => _cursor ?? new Cursor<TokenNode>(_children);

//    public TokenNode Add(TokenNode node) => this.Action(_ => _children.Add(node));

//    public IEnumerator<TokenNode> GetEnumerator() => _children.GetEnumerator();
//    IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();

//    public static IValidator<TokenNode> Validator { get; } = new Validator<TokenNode>()
//        .RuleForObject(x => x).Must(x =>
//        {
//            return x.Type switch
//            {
//                TokenNodeType.Root when x.Parent != null => Option<string>.None,
//                TokenNodeType.Root => "Parent is not null",

//                TokenNodeType.Literal when x.Symbol.IsEmpty() => Option<string>.None,
//                TokenNodeType.Literal => "Symbol should not be set for literal type",

//                TokenNodeType.Symbol when x.Symbol.IsNotEmpty() => Option<string>.None,
//                TokenNodeType.Symbol => "Symbol is required for token",

//                TokenNodeType.Value when x.Value.IsNotEmpty() => Option<string>.None,
//                TokenNodeType.Value => "Value is required for symbol",

//                _ => $"Invalid type={x.Type}",
//            };
//        })
//        .Build();
//}

//public static class TN
//{
//    public static TokenNode CreateRoot() => new TokenNode();

//    public static Option Validate(this TokenNode subject) => TokenNode.Validator.Validate(subject).ToOptionStatus();

//    public static bool Validate(this TokenNode subject, out Option result)
//    {
//        result = subject.Validate();
//        return result.IsOk();
//    }

//    public static TokenNode AddLiteral(this TokenNode subject, string? name = null)
//    {
//        subject.NotNull();

//        var newNode = new TokenNode(subject, TokenNodeType.Literal, name: name);
//        subject.Children.Add(newNode);

//        return newNode;
//    }

//    public static TokenNode AddSymbol(this TokenNode subject, string symbol, string? name = null)
//    {
//        subject.NotNull();
//        symbol.NotEmpty();

//        var newNode = new TokenNode(subject, TokenNodeType.Symbol, symbol: symbol, name: name);
//        subject.Children.Add(newNode);

//        return newNode;
//    }
//}
