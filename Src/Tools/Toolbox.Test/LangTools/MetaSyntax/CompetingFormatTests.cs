using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class CompetingFormatTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public CompetingFormatTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = , [ ] = { } ( ) ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "name                = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "open-brace          = '{' ;",
            "close-brace         = '}' ;",
            "open-param          = '(' #group-start #node;",
            "close-param         = ')' #group-end #node ;",
            "base64              = string ;",
            "term                = ';' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
            "entity-data         = name, open-brace, base64, close-brace ;",
            "node-spec           = open-param, tags, close-param ;",
            "select              = node-spec, [ entity-data ], term ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

}
