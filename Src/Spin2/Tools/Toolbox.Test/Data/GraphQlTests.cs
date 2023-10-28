using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;

namespace Toolbox.Test.Data;

public class GraphQlTests
{
    [Fact]
    public void NodeSyntax()
    {
        //var q = "(t1)";
        //var q = "(key=key1;tags=t1)";
        //var q = "[schedulework:active]";
        var q = "(key=key1;tags=t1) n1 -> [schedulework:active] -> (schedule) n2";

        var result = GraphQL.Parse(q);
    }
}
