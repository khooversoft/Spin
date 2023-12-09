namespace Toolbox.Types;

public interface ITimeContext
{
    DateTime GetUtc();
}


public class TimeContext : ITimeContext
{
    public DateTime GetUtc() => DateTime.UtcNow;
}
