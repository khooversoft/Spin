using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class ScopeContextTests
{
    [Fact]
    public void LoggingLocationTest()
    {
        var context = new ScopeContext(NullLogger.Instance);
        context.TraceId.Should().NotBeNullOrEmpty();

        Action testLogger = () => context.Logger.NotNull();
        testLogger.Should().NotThrow();

        ScopeContextLocation location = context.Location();
        location.Should().NotBeNull();
        location.Context.TraceId.Should().Be(context.TraceId);
    }
}
