using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class AddEdgeTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public AddEdgeTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        // add node key={key} set ( data | tag ), { comma, ( data | tag ) } ;
        // upsert node key={key} set ( data | tag ), { comma, ( data | tag ) } ;
        string schemaText = new[]
        {
            "delimiters          = = { } ';' , ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "name                = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "base64              = string ;",
            "tagKey              = symbol ;",
            "tagValue            = string ;",
            "dataName            = name ;",
            "comma               = ',' ;",
            "term                = ';' ;",
            "equal               = '=' ;",
            "set-sym             = 'set' ;",
            "add-sym             = 'add' ;",
            "upsert-sym          = 'upsert' ;",
            "update-sym          = 'update' ;",
            "select-sym          = 'select' ;",
            "delete-sym          = 'delete' ;",
            "node-sym            = 'node' ;",
            "key-sym             = 'key' ;",
            "",
            "open-brace          = '{' #group-start #data ;",
            "close-brace         = '}' #group-end #data ;",
            "keyValue            = string ;",
            "entity-data         = dataName, open-brace, base64, close-brace ;",
            "tag                 = tagKey, [ '=', tagValue ] ;",
            "",
            "command             = ( add-sym | upsert-sym | update-sym | select-sym | delete-sym ) ;",
            "set-data            = [ set-sym, ( entity-data | tag ), { comma, ( entity-data | tag ) } ] ;",
            "addNode             = command, node-sym, key-sym, equal, keyValue, set-data, term;",
            "",
        }.Join(Environment.NewLine);

        _root = MetaParser.ParseRules(schemaText);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(_output.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .BuildServiceProvider();

        var logger = services.GetService<ILogger<NodeAddTests>>().NotNull();
        _context = new ScopeContext(logger);

        _parser = new SyntaxParser(_root);
    }

    //[Theory]
    //[InlineData("")]
    //[InlineData("[]")]
    //[InlineData("[*])")]
    //[InlineData("[!*]")]
    //public void FailedReturn(string command)
    //{
    //    var parser = new SyntaxParser(_schema);
    //    var logger = GetScopeContext<OrRuleTests>();

    //    var parse = parser.Parse(command, logger);
    //    parse.StatusCode.IsError().Should().BeTrue(parse.Error);
    //}
}
