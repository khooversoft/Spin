using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Telemetry;
using Toolbox.Tools;

namespace Toolbox.Test.Telemetry;

public class TelemetryCollectorTests
{
    [Fact]
    public void EmptyConfiguration()
    {
        var option = new TelemetryOption();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(0);
    }

    [Fact]
    public void AddCollectorWithInstance_ShouldIncreaseCount()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(0);

        var added = collector.AddCollector(mockCollector);
        added.BeTrue();
        collector.Count.Be(1);
    }

    [Fact]
    public void AddCollectorWithSameType_ShouldReturnFalse()
    {
        var option = new TelemetryOption();
        var mockCollector1 = new MockTelemetryCollector();
        var mockCollector2 = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        var added1 = collector.AddCollector(mockCollector1);
        added1.BeTrue();
        collector.Count.Be(1);

        var added2 = collector.AddCollector(mockCollector2);
        added2.BeFalse();
        collector.Count.Be(1);
    }

    [Fact]
    public void PostEvent_ShouldCallAllCollectors()
    {
        var option = new TelemetryOption();
        var mockCollector1 = new MockTelemetryCollector();
        var mockCollector2 = new MockTelemetryCollector2();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.AddCollector(mockCollector1);
        collector.AddCollector(mockCollector2);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "test.event",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);

        mockCollector1.ReceivedEvents.Count.Be(1);
        mockCollector1.ReceivedEvents[0].Name.Be("test.event");
        mockCollector2.ReceivedEvents.Count.Be(1);
        mockCollector2.ReceivedEvents[0].Name.Be("test.event");
    }

    [Fact]
    public void AddCollectorWithName_ShouldAddAndReturnDisposable()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(0);

        var scope = collector.AddCollector("test.collector", mockCollector);
        scope.NotNull();
        collector.Count.Be(1);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "test.event",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);
        mockCollector.ReceivedEvents.Count.Be(1);

        scope.Dispose();
        collector.Count.Be(0);

        collector.Post(telemetryEvent);
        mockCollector.ReceivedEvents.Count.Be(1); // No new events after disposal
    }

    [Fact]
    public void TryRemoveCollector_ShouldRemoveExistingCollector()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.AddCollector("test.collector", mockCollector);
        collector.Count.Be(1);

        var removed = collector.TryRemoveCollector("test.collector");
        removed.BeTrue();
        collector.Count.Be(0);
    }

    [Fact]
    public void TryRemoveCollector_NonExistent_ShouldReturnFalse()
    {
        var option = new TelemetryOption();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(0);

        var removed = collector.TryRemoveCollector("nonexistent");
        removed.BeFalse();
    }

    [Fact]
    public void ConfigurationWithCollectors_ShouldInitializeFromOption()
    {
        var mockCollector = new MockTelemetryCollector();
        var option = new TelemetryOption()
            .AddCollector(mockCollector);

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(1);
    }

    [Fact]
    public void ConfigurationWithMultipleCollectors_ShouldInitializeAll()
    {
        var mockCollector1 = new MockTelemetryCollector();
        var mockCollector2 = new MockTelemetryCollector2();
        var option = new TelemetryOption()
            .AddCollector(mockCollector1)
            .AddCollector(mockCollector2);

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(2);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "test.event",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);
        mockCollector1.ReceivedEvents.Count.Be(1);
        mockCollector2.ReceivedEvents.Count.Be(1);
    }

    [Fact]
    public void AddCollectorWithName_DuplicateName_ShouldThrow()
    {
        var option = new TelemetryOption();
        var mockCollector1 = new MockTelemetryCollector();
        var mockCollector2 = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        collector.AddCollector("test.collector", mockCollector1);
        collector.Count.Be(1);

        Assert.Throws<ArgumentException>(() => collector.AddCollector("test.collector", mockCollector2));
    }

    [Fact]
    public void PostEvent_WithNoCollectors_ShouldNotThrow()
    {
        var option = new TelemetryOption();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(0);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "test.event",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);
    }

    [Fact]
    public void MultiplePostEvents_ShouldAccumulateInCollectors()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.AddCollector(mockCollector);

        var event1 = new TelemetryEvent
        {
            Name = "test.event1",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        var event2 = new TelemetryEvent
        {
            Name = "test.event2",
            Scope = "test.scope",
            EventType = "counter",
            Value = "2",
            ValueType = "Int32"
        };

        collector.Post(event1);
        collector.Post(event2);

        mockCollector.ReceivedEvents.Count.Be(2);
        mockCollector.ReceivedEvents[0].Name.Be("test.event1");
        mockCollector.ReceivedEvents[1].Name.Be("test.event2");
    }

    [Fact]
    public void Constructor_NullOption_ShouldThrow()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new TelemetryCollector(null!, services, NullLogger<TelemetryCollector>.Instance));
    }

    [Fact]
    public void Constructor_NullServiceProvider_ShouldThrow()
    {
        var option = new TelemetryOption();

        Assert.Throws<ArgumentNullException>(() => new TelemetryCollector(option, null!, NullLogger<TelemetryCollector>.Instance));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var option = new TelemetryOption();
        var services = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new TelemetryCollector(option, services, null!));
    }

    [Fact]
    public void AddCollector_NullInstance_ShouldThrow()
    {
        var option = new TelemetryOption();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        Assert.Throws<ArgumentNullException>(() => collector.AddCollector((ITelemetryCollector)null!));
    }

    [Fact]
    public void AddCollectorWithName_NullName_ShouldThrow()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        Assert.Throws<ArgumentNullException>(() => collector.AddCollector(null!, mockCollector));
    }

    [Fact]
    public void AddCollectorWithName_EmptyName_ShouldThrow()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        Assert.Throws<ArgumentNullException>(() => collector.AddCollector(string.Empty, mockCollector));
    }

    [Fact]
    public void AddCollectorWithName_NullInstance_ShouldThrow()
    {
        var option = new TelemetryOption();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        Assert.Throws<ArgumentNullException>(() => collector.AddCollector("test.collector", null!));
    }

    [Fact]
    public void TryRemoveCollector_ShouldBeCaseInsensitive()
    {
        var option = new TelemetryOption();
        var mockCollector = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.AddCollector("Test.Collector", mockCollector);

        var removed = collector.TryRemoveCollector("test.collector");
        removed.BeTrue();
        collector.Count.Be(0);
    }

    [Fact]
    public void AddCollectorWithName_CaseInsensitiveDuplicate_ShouldThrow()
    {
        var option = new TelemetryOption();
        var mockCollector1 = new MockTelemetryCollector();
        var mockCollector2 = new MockTelemetryCollector();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();

        collector.AddCollector("Test.Collector", mockCollector1);
        Assert.Throws<ArgumentException>(() => collector.AddCollector("test.collector", mockCollector2));
    }

    [Fact]
    public void ConfigurationWithServiceProviderCollectors_ShouldResolveDependencies()
    {
        var option = new TelemetryOption()
            .AddCollector<DependencyBackedCollector>();

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<CollectorDependency>()
            .AddSingleton<DependencyBackedCollector>()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(1);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "test.event",
            Scope = "test.scope",
            EventType = "counter",
            Value = "1",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);

        var dependency = di.GetRequiredService<CollectorDependency>();
        dependency.Events.Count.Be(1);
        dependency.Events[0].Name.Be("test.event");
    }

    [Fact]
    public void ConfigurationWithFactoryCollectors_ShouldInvokeFactoryAndForwardEvents()
    {
        var option = new TelemetryOption()
            .AddCollector(sp => new FactoryBackedCollector(sp.GetRequiredService<FactoryCollectorState>()));

        var di = new ServiceCollection()
            .AddLogging()
            .AddSingleton<FactoryCollectorState>()
            .AddSingleton<TelemetryCollector>()
            .AddSingleton(option)
            .BuildServiceProvider();

        var collector = di.GetRequiredService<TelemetryCollector>().NotNull();
        collector.Count.Be(1);

        var state = di.GetRequiredService<FactoryCollectorState>();
        state.InvocationCount.Be(1);

        var telemetryEvent = new TelemetryEvent
        {
            Name = "factory.event",
            Scope = "factory.scope",
            EventType = "counter",
            Value = "42",
            ValueType = "Int32"
        };

        collector.Post(telemetryEvent);

        state.Events.Count.Be(1);
        state.Events[0].Name.Be("factory.event");
    }

    private class MockTelemetryCollector : ITelemetryCollector
    {
        public List<TelemetryEvent> ReceivedEvents { get; } = new();

        public void Post(TelemetryEvent telemetryEvent)
        {
            ReceivedEvents.Add(telemetryEvent);
        }
    }

    private class MockTelemetryCollector2 : ITelemetryCollector
    {
        public List<TelemetryEvent> ReceivedEvents { get; } = new();

        public void Post(TelemetryEvent telemetryEvent)
        {
            ReceivedEvents.Add(telemetryEvent);
        }
    }

    private sealed class DependencyBackedCollector : ITelemetryCollector
    {
        private readonly CollectorDependency _dependency;

        public DependencyBackedCollector(CollectorDependency dependency) => _dependency = dependency.NotNull();

        public void Post(TelemetryEvent telemetryEvent)
        {
            _dependency.Events.Add(telemetryEvent);
        }
    }

    private sealed class FactoryBackedCollector : ITelemetryCollector
    {
        private readonly FactoryCollectorState _state;

        public FactoryBackedCollector(FactoryCollectorState state)
        {
            _state = state.NotNull();
            _state.InvocationCount++;
        }

        public void Post(TelemetryEvent telemetryEvent)
        {
            _state.Events.Add(telemetryEvent);
        }
    }

    private sealed class CollectorDependency
    {
        public List<TelemetryEvent> Events { get; } = new();
    }

    private sealed class FactoryCollectorState
    {
        public int InvocationCount { get; set; }

        public List<TelemetryEvent> Events { get; } = new();
    }
}
