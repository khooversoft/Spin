using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ScopeContextTests
{
    [Fact]
    public void LoggingLocationTest()
    {
        var context = new ScopeContext(NullLogger.Instance);
        context.TraceId.NotEmpty();
        context.Logger.NotNull();

        ScopeContextLocation location = context.Location();
        location.NotNull();
        location.Context.TraceId.Be(context.TraceId);
    }
}
