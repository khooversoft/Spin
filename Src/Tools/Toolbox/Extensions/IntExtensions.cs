namespace Toolbox.Extensions;

public static class IntExtensions
{
    public static bool IsWithinPercentage(this int subject, int target, double percentage)
    {
        if (percentage <= 0) throw new ArgumentException("Percentage must be non-negative.");

        double lowerBound = target - (target * percentage / 100);
        double upperBound = target + (target * percentage / 100);

        return subject >= lowerBound && subject <= upperBound;
    }
}
