namespace Toolbox.Metrics;

public interface IMetric
{
    void TrackValue(string name, double value);
}
