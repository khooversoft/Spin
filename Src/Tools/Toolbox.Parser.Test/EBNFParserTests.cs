﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Parser.Grammar;
using Toolbox.Parser.Syntax;
using Toolbox.Types.Structure;

namespace Toolbox.Parser.Test;

public class EBNFParserTests
{
    private static string _code = """
        letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
               | "H" | "I" | "J" | "K" | "L" | "M" | "N"
               | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
               | "V" | "W" | "X" | "Y" | "Z" | "a" | "b"
               | "c" | "d" | "e" | "f" | "g" | "h" | "i"
               | "j" | "k" | "l" | "m" | "n" | "o" | "p"
               | "q" | "r" | "s" | "t" | "u" | "v" | "w"
               | "x" | "y" | "z" ;
        digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;
        symbol = "[" | "]" | "{" | "}" | "(" | ")" | "<" | ">"
               | "'" | '"' | "=" | "|" | "." | "," | ";" ;
        character = letter | digit | symbol | "_" ;

        identifier = letter , { letter | digit | "_" } ;
        terminal = "'" , character - "'" , { character - "'" } , "'" 
                 | '"' , character - '"' , { character - '"' } , '"' ;

        lhs = identifier ;
        rhs = identifier
             | terminal
             | "[" , rhs , "]"
             | "{" , rhs , "}"
             | "(" , rhs , ")"
             | rhs , "|" , rhs
             | rhs , "," , rhs ;

        rule = lhs , "=" , rhs , ";" ;
        grammar = { rule } ;
        """;

    [Fact]
    public void ParserOr()
    {
        string code = """
            letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
                   | "H" | "I" | "J" | "K" | "L" | "M" | "N"
                   | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
                   | "V" | "W" | "X" | "Y" | "Z" | "a" | "b"
                   | "c" | "d" | "e" | "f" | "g" | "h" | "i"
                   | "j" | "k" | "l" | "m" | "n" | "o" | "p"
                   | "q" | "r" | "s" | "t" | "u" | "v" | "w"
                   | "x" | "y" | "z" ;
            """;

        var grammarTree = new Tree()
            + ( new RuleNode("rule")
                + new LiteralRule()
                + new TokenRule("=")
                + new LiteralRule(LiteralType.String)
                + (new RepeatRule() + new TokenRule("|") + new LiteralRule(LiteralType.String))
                + new TokenRule(";")
            );

        Tree? syntaxTree = new SyntaxTreeBuilder().Build(code, grammarTree);
        syntaxTree.Should().NotBeNull();
    }
}
