using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ScopeContextTests
{
    [Fact]
    public void LoggingLocationTest()
    {
        var context = new ScopeContext(NullLogger.Instance);
        context.TraceId.Should().NotBeEmpty();

        Action testLogger = () => context.Logger.NotNull();
        testLogger.Should().NotThrow();

        ScopeContextLocation location = context.Location();
        location.NotNull();
        location.Context.TraceId.Should().Be(context.TraceId);
    }
}
