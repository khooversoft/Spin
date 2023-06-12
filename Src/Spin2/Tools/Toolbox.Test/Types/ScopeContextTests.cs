using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ScopeContextTests
{
    [Fact]
    public void LoggingLocationTest()
    {
        var context = new ScopeContext(NullLogger.Instance);
        ScopeContextLocation location = context.Location();
    }
}
