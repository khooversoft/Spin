using System.Diagnostics.Metrics;

namespace Toolbox.Test.Framework;

public class OpenTelemetryTest
{
    [Fact]
    public void Counter_Should_Record_Expected_Value()
    {
        // Arrange
        var meter = new Meter("TestMeter");
        var counter = meter.CreateCounter<long>("test_counter");

        long observedValue = 0;
        var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Name == "test_counter")
                listener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((inst, value, tags, state) =>
        {
            observedValue += value;
        });
        listener.Start();

        // Act
        counter.Add(5);
        counter.Add(3);

        // Assert
        Assert.Equal(8, observedValue);

        listener.Dispose();
        meter.Dispose();
    }
}