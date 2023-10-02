using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Finance.Finance;

public static class ScheduleTool
{
    public static IReadOnlyList<DateTime> CalculatePaymentSchedule(DateTime startDate, int numPayments, int paymentsPerYear)
    {
        Func<DateTime, DateTime> calc = paymentsPerYear switch
        {
            1 => x => x.AddYears(1),
            2 => x => x.AddMonths(6),
            3 => x => x.AddMonths(4),
            4 => x => x.AddMonths(3),
            12 => x => x.AddMonths(1),
            26 => x => x.AddDays(14),
            52 => x => x.AddDays(7),
            int v => x => x.AddDays(365 / v),
        };

        DateTime current = startDate;

        var list = Enumerable.Range(0, numPayments - 1)
            .Select(_ => current = calc(current))
            .Prepend(startDate)
            .ToArray();

        return list;
    }
}
