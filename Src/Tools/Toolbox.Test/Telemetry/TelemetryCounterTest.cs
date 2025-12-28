using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Telemetry;

public class TelemetryCounterTest
{
    private ITestOutputHelper _outputHelper;
    public TelemetryCounterTest(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    private IHost BuildService() => Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            services.AddLogging(c => c.AddLambda(_outputHelper.WriteLine).AddDebug().AddFilter(_ => true));

            services.AddSingleton<TelemetryAggregator>();
            services.AddTelemetry(config =>
            {
                config.AddCollector<TelemetryAggregator>();
            });
        })
        .Build();

    [Fact]
    public void SimpleCounterTest()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        listener.GetAllEvents().Count.Be(0);

        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.counter", "test counter", "count", "t1,t2=v2", "1.0");

        counter.Add(1, "scope1", "tag1=v1");
        listener.GetAllEvents().Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Name.Be("test.counter");
                x.Value.Be("1");
                x.Scope.Be("scope1");
                x.Tags.Be("t1,t2=v2,tag1=v1");
                x.Description.Be("test counter");
                x.Version.Be("1.0");
            });
        });

        var count = listener.GetCounterValue("test.counter");
        count.Be(1);

        listener.GetEventsByName("test.counter").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Name.Be("test.counter");
                x.Value.Be("1");
                x.Scope.Be("scope1");
                x.Tags.Be("t1,t2=v2,tag1=v1");
                x.Description.Be("test counter");
                x.Version.Be("1.0");
            });
        });
    }

    [Fact]
    public void CounterWithoutScopeShouldUseDefinitionDefaults()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<long>("test.counter.defaultScope", "default scope counter", "items", "definition=tag", "1.0");

        counter.Add(5);

        listener.GetEventsByName("test.counter.defaultScope").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Scope.BeEmpty();
                x.Tags.Be("definition=tag");
                x.Value.Be("5");
                x.ValueType.Be("Int64");
                x.Units.Be("items");
            });
        });
    }

    [Fact]
    public void MultipleCounterPostsShouldAccumulateValues()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.accumulator", "accumulator counter", "count");

        counter.Add(10);
        counter.Add(20);
        counter.Add(30);

        listener.GetAllEvents().Count.Be(3);
        listener.GetCounterValue("test.accumulator").Be(60);
    }

    [Fact]
    public void CounterWithLongTypeShouldAccumulateCorrectly()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<long>("test.long.counter", "long counter", "bytes");

        counter.Add(1000000000L);
        counter.Add(2000000000L);

        listener.GetCounterValue("test.long.counter").Be(3000000000L);
        listener.GetEventsByName("test.long.counter").Action(e1 =>
        {
            e1.Count.Be(2);
            e1[0].ValueType.Be("Int64");
        });
    }

    [Fact]
    public void CounterWithNoTagsShouldHaveNullTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.notags", "no tags counter", "items");

        counter.Add(5);

        listener.GetEventsByName("test.notags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.BeNull();
        });
    }

    [Fact]
    public void CounterWithOnlyDefinitionTagsShouldPreserveTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.deftags", "def tags counter", "items", "env=prod,region=east");

        counter.Add(1);

        listener.GetEventsByName("test.deftags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.Be("env=prod,region=east");
        });
    }

    [Fact]
    public void CounterWithOnlyPostTagsShouldUseTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.posttags", "post tags counter", "items");

        counter.Add(1, null, "request=abc123");

        listener.GetEventsByName("test.posttags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.Be("request=abc123");
        });
    }

    [Fact]
    public void ListenerClearShouldRemoveAllEvents()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.clear", "clear counter", "items");

        counter.Add(10);
        counter.Add(20);

        listener.GetAllEvents().Count.Be(2);
        listener.GetCounterValue("test.clear").Be(30);

        listener.Clear();

        listener.GetAllEvents().Count.Be(0);
        listener.GetCounterValue("test.clear").Be(-1);
        listener.GetEventsByName("test.clear").Count.Be(0);
    }

    [Fact]
    public void GetCounterValueForNonExistentCounterShouldReturnNegativeOne()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        long result = listener.GetCounterValue("nonexistent.counter");
        result.Be(-1);
    }

    [Fact]
    public void GetEventsByNameForNonExistentNameShouldReturnEmptyList()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        var result = listener.GetEventsByName("nonexistent.event");
        result.Count.Be(0);
    }

    //[Fact]
    //public void HistogramShouldRecordEventsCorrectly()
    //{
    //    using var host = BuildService();

    //    var listener = host.Services.GetRequiredService<TelemetryListener>();
    //    ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
    //    var histogram = telemetry.CreateHistogram<int>("test.histogram", "response time histogram", "ms", "service=api", "2.0");

    //    histogram.Add(150, "request1");
    //    histogram.Add(200, "request2");

    //    listener.GetAllEvents().Count.Be(2);
    //    listener.GetEventsByName("test.histogram").Action(e1 =>
    //    {
    //        e1.Count.Be(2);
    //        e1[0].Action(x =>
    //        {
    //            x.EventType.Be(MetricDefinition.HistogramType);
    //            x.Value.Be("150");
    //            x.Scope.Be("test.histogram.request1");
    //            x.Units.Be("ms");
    //        });
    //        e1[1].Action(x =>
    //        {
    //            x.Value.Be("200");
    //            x.Scope.Be("test.histogram.request2");
    //        });
    //    });
    //}

    //[Fact]
    //public void GaugeShouldRecordEventsCorrectly()
    //{
    //    using var host = BuildService();

    //    var listener = host.Services.GetRequiredService<TelemetryListener>();
    //    ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
    //    var gauge = telemetry.CreateGauge<int>("test.gauge", "memory usage gauge", "MB", "host=server1", "1.0");

    //    gauge.Add(512);
    //    gauge.Add(768);

    //    listener.GetEventsByName("test.gauge").Action(e1 =>
    //    {
    //        e1.Count.Be(2);
    //        e1[0].Action(x =>
    //        {
    //            x.EventType.Be(MetricDefinition.GaugeType);
    //            x.Value.Be("512");
    //            x.Units.Be("MB");
    //            x.Tags.Be("host=server1");
    //        });
    //        e1[1].Value.Be("768");
    //    });
    //}

    [Fact]
    public void MultipleCountersWithDifferentNamesShouldBeTrackedSeparately()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();

        var counter1 = telemetry.CreateCounter<int>("counter.one", "first counter", "items");
        var counter2 = telemetry.CreateCounter<int>("counter.two", "second counter", "requests");

        counter1.Add(5);
        counter1.Add(10);
        counter2.Add(100);

        listener.GetAllEvents().Count.Be(3);
        listener.GetCounterValue("counter.one").Be(15);
        listener.GetCounterValue("counter.two").Be(100);
        listener.GetEventsByName("counter.one").Count.Be(2);
        listener.GetEventsByName("counter.two").Count.Be(1);
    }

    [Fact]
    public void CounterEventShouldHaveTimestampAndId()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.metadata", "metadata counter", "items");

        DateTime beforePost = DateTime.UtcNow;
        counter.Add(1);
        DateTime afterPost = DateTime.UtcNow;

        listener.GetEventsByName("test.metadata").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Id.NotNull();
                Guid.TryParse(x.Id, out var _).BeTrue();
                (x.Timestamp >= beforePost && x.Timestamp <= afterPost).BeTrue();
            });
        });
    }

    [Fact]
    public void CounterWithNullValueShouldPostNullString()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.value", "value counter", "items");

        counter.Add(0);

        listener.GetEventsByName("test.value").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Value.Be("0");
        });
    }

    [Fact]
    public void CounterEventTypeShouldBeCounter()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.eventtype", "event type counter", "items");

        counter.Increment();

        listener.GetEventsByName("test.eventtype").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].EventType.Be(MetricDefinition.CounterType);
        });
    }

    [Fact]
    public void CounterWithNoDescriptionShouldHaveNullDescription()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.nodesc");

        counter.Add(1);

        listener.GetEventsByName("test.nodesc").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Description.BeNull();
            e1[0].Units.Be("count");
            e1[0].Version.BeNull();
        });
    }

    [Fact]
    public void CounterWithNoVersionShouldHaveNullVersion()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var counter = telemetry.CreateCounter<int>("test.noversion", "no version counter", "items", "tag=value");

        counter.Add(1);

        listener.GetEventsByName("test.noversion").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Version.BeNull();
        });
    }
}
