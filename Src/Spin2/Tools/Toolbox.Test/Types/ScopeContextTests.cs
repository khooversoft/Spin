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
