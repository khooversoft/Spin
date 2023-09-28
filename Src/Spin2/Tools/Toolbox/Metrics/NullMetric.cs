namespace Toolbox.Metrics;

public class NullMetric : IMetric
{
    public static IMetric Default { get; } = new NullMetric();

    public void TrackValue(string name, double value) { }
}
