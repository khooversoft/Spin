using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ScopeContextTests
{
    [Fact]
    public void LoggingLocationTest()
    {
        var context = new ScopeContext();
        ScopeContextLocation location = context.Location();
    }
}
