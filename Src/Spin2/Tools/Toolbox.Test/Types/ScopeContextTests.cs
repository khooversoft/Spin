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

    [Fact]
    public void SerializationTest()
    {
        var context = new ScopeContext(NullLogger.Instance);

        string json = context.ToJson();

        ScopeContext c2 = json.ToObject<ScopeContext>();

        (context.TraceId == c2.TraceId).Should().BeTrue();

        Action testLogger2 = () => c2.Logger.NotNull();
        testLogger2.Should().Throw<ArgumentNullException>();
    }
}
