using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Telemetry;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Telemetry;

public class TelemetryGaugeTest
{
    private ITestOutputHelper _outputHelper;
    public TelemetryGaugeTest(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

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
    public void SimpleGaugeTest()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        listener.GetAllEvents().Count.Be(0);

        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<long>("test.gauge", "test gauge", "count", "t1,t2=v2", "1.0");

        gauge.Post(1, "scope1", "tag1=v1");
        listener.GetAllEvents().Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Name.Be("test.gauge");
                x.Value.Be("1");
                x.Scope.Be("scope1");
                x.Tags.Be("t1,t2=v2,tag1=v1");
                x.Description.Be("test gauge");
                x.Version.Be("1.0");
            });
        });

        var count = listener.GetGaugeValue("test.gauge");
        count.Be(1);

        listener.GetEventsByName("test.gauge").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Action(x =>
            {
                x.Name.Be("test.gauge");
                x.Value.Be("1");
                x.Scope.Be("scope1");
                x.Tags.Be("t1,t2=v2,tag1=v1");
                x.Description.Be("test gauge");
                x.Version.Be("1.0");
            });
        });
    }

    [Fact]
    public void GaugeWithoutScopeShouldUseDefinitionDefaults()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<long>("test.gauge.defaultScope", "default scope gauge", "items", "definition=tag", "1.0");

        gauge.Post(5);

        listener.GetEventsByName("test.gauge.defaultScope").Action(e1 =>
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
    public void MultipleGaugePostsShouldReplaceValues()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.gauge.replace", "replacement gauge", "units");

        gauge.Post(10);
        gauge.Post(20);
        gauge.Post(30);

        listener.GetAllEvents().Count.Be(3);
        // Gauge should replace, not accumulate - last value should be 30
        listener.GetGaugeValue("test.gauge.replace").Be(30);
    }

    [Fact]
    public void GaugeWithIntTypeShouldRecordCorrectly()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.int.gauge", "int gauge", "percentage");

        gauge.Post(50);
        gauge.Post(75);

        listener.GetGaugeValue("test.int.gauge").Be(75);
        listener.GetEventsByName("test.int.gauge").Action(e1 =>
        {
            e1.Count.Be(2);
            e1[0].ValueType.Be("Int32");
            e1[1].ValueType.Be("Int32");
        });
    }

    [Fact]
    public void GaugeWithLongTypeShouldRecordCorrectly()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<long>("test.long.gauge", "long gauge", "bytes");

        gauge.Post(1000000000L);
        gauge.Post(2000000000L);

        listener.GetGaugeValue("test.long.gauge").Be(2000000000L);
        listener.GetEventsByName("test.long.gauge").Action(e1 =>
        {
            e1.Count.Be(2);
            e1[0].ValueType.Be("Int64");
        });
    }

    [Fact]
    public void GaugeWithNoTagsShouldHaveNullTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.notags", "no tags gauge", "items");

        gauge.Post(5);

        listener.GetEventsByName("test.notags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.BeNull();
        });
    }

    [Fact]
    public void GaugeWithOnlyDefinitionTagsShouldPreserveTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.deftags", "def tags gauge", "items", "env=prod,region=east");

        gauge.Post(1);

        listener.GetEventsByName("test.deftags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.Be("env=prod,region=east");
        });
    }

    [Fact]
    public void GaugeWithOnlyPostTagsShouldUseTags()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.posttags", "post tags gauge", "items");

        gauge.Post(1, null, "request=abc123");

        listener.GetEventsByName("test.posttags").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Tags.Be("request=abc123");
        });
    }

    [Fact]
    public void ListenerClearShouldRemoveAllGaugeEvents()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.clear", "clear gauge", "items");

        gauge.Post(10);
        gauge.Post(20);

        listener.GetAllEvents().Count.Be(2);
        listener.GetGaugeValue("test.clear").Be(20);

        listener.Clear();

        listener.GetAllEvents().Count.Be(0);
        listener.GetGaugeValue("test.clear").Be(20);
        listener.GetEventsByName("test.clear").Count.Be(0);
    }

    [Fact]
    public void GetGaugeValueForNonExistentGaugeShouldReturnNegativeOne()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();

        long result = listener.GetGaugeValue("nonexistent.gauge");
        result.Be(-1);
    }

    [Fact]
    public void MultipleGaugesWithDifferentNamesShouldBeTrackedSeparately()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();

        var gauge1 = telemetry.CreateGauge<int>("gauge.one", "first gauge", "items");
        var gauge2 = telemetry.CreateGauge<int>("gauge.two", "second gauge", "requests");

        gauge1.Post(5);
        gauge1.Post(10);
        gauge2.Post(100);

        listener.GetAllEvents().Count.Be(3);
        listener.GetGaugeValue("gauge.one").Be(10);
        listener.GetGaugeValue("gauge.two").Be(100);
        listener.GetEventsByName("gauge.one").Count.Be(2);
        listener.GetEventsByName("gauge.two").Count.Be(1);
    }

    [Fact]
    public void GaugeEventShouldHaveTimestampAndId()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.metadata", "metadata gauge", "items");

        DateTime beforePost = DateTime.UtcNow;
        gauge.Post(1);
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
    public void GaugeWithZeroValueShouldPostCorrectly()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.zero", "zero gauge", "items");

        gauge.Post(0);

        listener.GetEventsByName("test.zero").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Value.Be("0");
        });
    }

    [Fact]
    public void GaugeEventTypeShouldBeGauge()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.eventtype", "event type gauge", "items");

        gauge.Post(1);

        listener.GetEventsByName("test.eventtype").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].EventType.Be(MetricDefinition.GaugeType);
        });
    }

    [Fact]
    public void GaugeWithNoDescriptionShouldHaveNullDescription()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.nodesc");

        gauge.Post(1);

        listener.GetEventsByName("test.nodesc").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Description.BeNull();
            e1[0].Version.BeNull();
        });
    }

    [Fact]
    public void GaugeWithNoVersionShouldHaveNullVersion()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.noversion", "no version gauge", "items", "tag=value");

        gauge.Post(1);

        listener.GetEventsByName("test.noversion").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Version.BeNull();
        });
    }

    [Fact]
    public void GaugeDefaultUnitShouldBeGauge()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.defaultunit", "default unit gauge");

        gauge.Post(1);

        listener.GetEventsByName("test.defaultunit").Action(e1 =>
        {
            e1.Count.Be(1);
            e1[0].Units.Be("gauge");
        });
    }

    [Fact]
    public void GaugeReplacementBehaviorShouldNotAccumulate()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.replacement", "replacement gauge", "units");

        gauge.Post(100);
        listener.GetGaugeValue("test.replacement").Be(100);

        gauge.Post(50);
        listener.GetGaugeValue("test.replacement").Be(50);

        gauge.Post(75);
        listener.GetGaugeValue("test.replacement").Be(75);

        // All 3 events should be recorded
        listener.GetAllEvents().Count.Be(3);
        // But the gauge value should be the last posted value
        listener.GetGaugeValue("test.replacement").Be(75);
    }

    [Fact]
    public void GaugeWithDifferentScopesShouldRecordAllEvents()
    {
        using var host = BuildService();

        var listener = host.Services.GetRequiredService<TelemetryAggregator>();
        ITelemetry telemetry = host.Services.GetRequiredService<ITelemetry>();
        var gauge = telemetry.CreateGauge<int>("test.scopes", "scope gauge", "items");

        gauge.Post(10, "scope1");
        gauge.Post(20, "scope2");
        gauge.Post(30, "scope3");

        listener.GetEventsByName("test.scopes").Action(e1 =>
        {
            e1.Count.Be(3);
            e1[0].Scope.Be("scope1");
            e1[1].Scope.Be("scope2");
            e1[2].Scope.Be("scope3");
        });

        listener.GetGaugeValue("test.scopes").Be(30);
    }
}
