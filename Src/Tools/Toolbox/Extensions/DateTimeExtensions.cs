namespace Toolbox.Extensions;

public static class DateTimeExtensions
{
    private static readonly DateTime _minRange = new DateTime(1900, 1, 1);
    private static readonly DateTime _maxRange = new DateTime(2199, 12, 31);
    private static readonly DateOnly _minDateOnlyRange = new DateOnly(1900, 1, 1);
    private static readonly DateOnly _maxDateOnlyRange = new DateOnly(2199, 12, 31);

    public static bool IsDateTimeValid(this DateTime? date) => date switch
    {
        DateTime v => v.IsDateTimeValid(),
        _ => false,
    };

    public static bool IsDateTimeValid(this DateTime date) => date >= _minRange && date <= _maxRange;

    public static bool IsDateOnlyValid(this DateOnly? date) => date switch
    {
        DateOnly v => v.IsDateOnlyValid(),
        _ => false,
    };

    public static bool IsDateOnlyValid(this DateOnly date) => date >= _minDateOnlyRange && date <= _maxDateOnlyRange;
}
