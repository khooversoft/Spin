using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Finance.Finance;

namespace Toolbox.Finance.test;

public class ScheduleToolTests
{
    [Fact]
    public void MonthlyPaymentScheduleCalculationTest()
    {
        var startDate = new DateTime(2000, 1, 1);
        int paymentsPerYear = 12;
        int numberOfPayments = 36;

        IReadOnlyList<DateTime> dates = ScheduleTool.CalculatePaymentSchedule(startDate, numberOfPayments, paymentsPerYear);
        dates.Should().NotBeNull();
        dates.Count.Should().Be(numberOfPayments);

        var shouldMatch = new[]
        {
            new DateTime(2000, 1, 1), new DateTime(2000, 2, 1), new DateTime(2000, 3, 1), new DateTime(2000, 4, 1),
            new DateTime(2000, 5, 1), new DateTime(2000, 6, 1), new DateTime(2000, 7, 1), new DateTime(2000, 8, 1),
            new DateTime(2000, 9, 1), new DateTime(2000, 10, 1), new DateTime(2000, 11, 1), new DateTime(2000, 12, 1),
            new DateTime(2001, 1, 1), new DateTime(2001, 2, 1), new DateTime(2001, 3, 1), new DateTime(2001, 4, 1),
            new DateTime(2001, 5, 1), new DateTime(2001, 6, 1), new DateTime(2001, 7, 1), new DateTime(2001, 8, 1),
            new DateTime(2001, 9, 1), new DateTime(2001, 10, 1), new DateTime(2001, 11, 1), new DateTime(2001, 12, 1),
            new DateTime(2002, 1, 1), new DateTime(2002, 2, 1), new DateTime(2002, 3, 1), new DateTime(2002, 4, 1),
            new DateTime(2002, 5, 1), new DateTime(2002, 6, 1), new DateTime(2002, 7, 1), new DateTime(2002, 8, 1),
            new DateTime(2002, 9, 1), new DateTime(2002, 10, 1), new DateTime(2002, 11, 1), new DateTime(2002, 12, 1),
        };

        Enumerable.SequenceEqual(dates, shouldMatch).Should().BeTrue();
        dates.Count.Should().Be(shouldMatch.Length);
    }

    [Fact]
    public void YearlyPaymentScheduleCalculationTest()
    {
        var startDate = new DateTime(2000, 1, 1);
        int paymentsPerYear = 1;
        int numberOfPayments = 5;

        IReadOnlyList<DateTime> dates = ScheduleTool.CalculatePaymentSchedule(startDate, numberOfPayments, paymentsPerYear);
        dates.Should().NotBeNull();
        dates.Count.Should().Be(numberOfPayments);

        var shouldMatch = new[]
        {
            new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2002, 1, 1), new DateTime(2003, 1, 1),
            new DateTime(2004, 1, 1),
        };

        Enumerable.SequenceEqual(dates, shouldMatch).Should().BeTrue();
        dates.Count.Should().Be(shouldMatch.Length);
    }

    [Fact]
    public void QuarterPaymentScheduleCalculationTest()
    {
        var startDate = new DateTime(2000, 1, 1);
        int paymentsPerYear = 4;
        int numberOfPayments = (2 * paymentsPerYear);

        IReadOnlyList<DateTime> dates = ScheduleTool.CalculatePaymentSchedule(startDate, numberOfPayments, paymentsPerYear);
        dates.Should().NotBeNull();
        dates.Count.Should().Be(numberOfPayments);

        var shouldMatch = new[]
        {
            new DateTime(2000, 1, 1), new DateTime(2000, 4, 1), new DateTime(2000, 7, 1), new DateTime(2000, 10, 1),
            new DateTime(2001, 1, 1), new DateTime(2001, 4, 1), new DateTime(2001, 7, 1), new DateTime(2001, 10, 1),
        };

        Enumerable.SequenceEqual(dates, shouldMatch).Should().BeTrue();
        dates.Count.Should().Be(shouldMatch.Length);
    }

    [Fact]
    public void BiWeeklyPaymentScheduleCalculationTest()
    {
        var startDate = new DateTime(2000, 1, 1);
        int paymentsPerYear = 26;
        int numberOfPayments = (2 * paymentsPerYear);

        IReadOnlyList<DateTime> dates = ScheduleTool.CalculatePaymentSchedule(startDate, numberOfPayments, paymentsPerYear);
        dates.Should().NotBeNull();
        dates.Count.Should().Be(numberOfPayments);

        var shouldMatch = new[]
        {
            new DateTime(2000, 1, 1), new DateTime(2000, 1, 15), new DateTime(2000, 1, 29), new DateTime(2000, 2, 12),
            new DateTime(2000, 2, 26), new DateTime(2000, 3, 11), new DateTime(2000, 3, 25), new DateTime(2000, 4, 8),
            new DateTime(2000, 4, 22), new DateTime(2000, 5, 6), new DateTime(2000, 5, 20), new DateTime(2000, 6, 3),
            new DateTime(2000, 6, 17), new DateTime(2000, 7, 1), new DateTime(2000, 7, 15), new DateTime(2000, 7, 29),
            new DateTime(2000, 8, 12), new DateTime(2000, 8, 26), new DateTime(2000, 9, 9), new DateTime(2000, 9, 23),
            new DateTime(2000, 10, 7), new DateTime(2000, 10, 21), new DateTime(2000, 11, 4), new DateTime(2000, 11, 18),
            new DateTime(2000, 12, 2), new DateTime(2000, 12, 16), new DateTime(2000, 12, 30), new DateTime(2001, 1, 13),
            new DateTime(2001, 1, 27), new DateTime(2001, 2, 10), new DateTime(2001, 2, 24), new DateTime(2001, 3, 10),
            new DateTime(2001, 3, 24), new DateTime(2001, 4, 7), new DateTime(2001, 4, 21), new DateTime(2001, 5, 5),
            new DateTime(2001, 5, 19), new DateTime(2001, 6, 2), new DateTime(2001, 6, 16), new DateTime(2001, 6, 30),
            new DateTime(2001, 7, 14), new DateTime(2001, 7, 28), new DateTime(2001, 8, 11), new DateTime(2001, 8, 25),
            new DateTime(2001, 9, 8), new DateTime(2001, 9, 22), new DateTime(2001, 10, 6), new DateTime(2001, 10, 20),
            new DateTime(2001, 11, 3), new DateTime(2001, 11, 17), new DateTime(2001, 12, 1), new DateTime(2001, 12, 15),
        };

        Enumerable.SequenceEqual(dates, shouldMatch).Should().BeTrue();
        dates.Count.Should().Be(shouldMatch.Length);
    }

    [Fact]
    public void WeeklyPaymentScheduleCalculationTest()
    {
        var startDate = new DateTime(2000, 1, 1);
        int paymentsPerYear = 52;
        int numberOfPayments = (2 * paymentsPerYear);

        IReadOnlyList<DateTime> dates = ScheduleTool.CalculatePaymentSchedule(startDate, numberOfPayments, paymentsPerYear);
        dates.Should().NotBeNull();
        dates.Count.Should().Be(numberOfPayments);

        var shouldMatch = new[]
        {
            new DateTime(2000, 1, 1), new DateTime(2000, 1, 8), new DateTime(2000, 1, 15), new DateTime(2000, 1, 22),
            new DateTime(2000, 1, 29), new DateTime(2000, 2, 5), new DateTime(2000, 2, 12), new DateTime(2000, 2, 19),
            new DateTime(2000, 2, 26), new DateTime(2000, 3, 4), new DateTime(2000, 3, 11), new DateTime(2000, 3, 18),
            new DateTime(2000, 3, 25), new DateTime(2000, 4, 1), new DateTime(2000, 4, 8), new DateTime(2000, 4, 15),
            new DateTime(2000, 4, 22), new DateTime(2000, 4, 29), new DateTime(2000, 5, 6), new DateTime(2000, 5, 13),
            new DateTime(2000, 5, 20), new DateTime(2000, 5, 27), new DateTime(2000, 6, 3), new DateTime(2000, 6, 10),
            new DateTime(2000, 6, 17), new DateTime(2000, 6, 24), new DateTime(2000, 7, 1), new DateTime(2000, 7, 8),
            new DateTime(2000, 7, 15), new DateTime(2000, 7, 22), new DateTime(2000, 7, 29), new DateTime(2000, 8, 5),
            new DateTime(2000, 8, 12), new DateTime(2000, 8, 19), new DateTime(2000, 8, 26), new DateTime(2000, 9, 2),
            new DateTime(2000, 9, 9), new DateTime(2000, 9, 16), new DateTime(2000, 9, 23), new DateTime(2000, 9, 30),
            new DateTime(2000, 10, 7), new DateTime(2000, 10, 14), new DateTime(2000, 10, 21), new DateTime(2000, 10, 28),
            new DateTime(2000, 11, 4), new DateTime(2000, 11, 11), new DateTime(2000, 11, 18), new DateTime(2000, 11, 25),
            new DateTime(2000, 12, 2), new DateTime(2000, 12, 9), new DateTime(2000, 12, 16), new DateTime(2000, 12, 23),
            new DateTime(2000, 12, 30), new DateTime(2001, 1, 6), new DateTime(2001, 1, 13), new DateTime(2001, 1, 20),
            new DateTime(2001, 1, 27), new DateTime(2001, 2, 3), new DateTime(2001, 2, 10), new DateTime(2001, 2, 17),
            new DateTime(2001, 2, 24), new DateTime(2001, 3, 3), new DateTime(2001, 3, 10), new DateTime(2001, 3, 17),
            new DateTime(2001, 3, 24), new DateTime(2001, 3, 31), new DateTime(2001, 4, 7), new DateTime(2001, 4, 14),
            new DateTime(2001, 4, 21), new DateTime(2001, 4, 28), new DateTime(2001, 5, 5), new DateTime(2001, 5, 12),
            new DateTime(2001, 5, 19), new DateTime(2001, 5, 26), new DateTime(2001, 6, 2), new DateTime(2001, 6, 9),
            new DateTime(2001, 6, 16), new DateTime(2001, 6, 23), new DateTime(2001, 6, 30), new DateTime(2001, 7, 7),
            new DateTime(2001, 7, 14), new DateTime(2001, 7, 21), new DateTime(2001, 7, 28), new DateTime(2001, 8, 4),
            new DateTime(2001, 8, 11), new DateTime(2001, 8, 18), new DateTime(2001, 8, 25), new DateTime(2001, 9, 1),
            new DateTime(2001, 9, 8), new DateTime(2001, 9, 15), new DateTime(2001, 9, 22), new DateTime(2001, 9, 29),
            new DateTime(2001, 10, 6), new DateTime(2001, 10, 13), new DateTime(2001, 10, 20), new DateTime(2001, 10, 27),
            new DateTime(2001, 11, 3), new DateTime(2001, 11, 10), new DateTime(2001, 11, 17), new DateTime(2001, 11, 24),
            new DateTime(2001, 12, 1), new DateTime(2001, 12, 8), new DateTime(2001, 12, 15), new DateTime(2001, 12, 22),
        };

        Enumerable.SequenceEqual(dates, shouldMatch).Should().BeTrue();
        dates.Count.Should().Be(shouldMatch.Length);
    }
}
