using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types;

public interface ITimeContext
{
    DateTime GetUtc();
}


public class TimeContext : ITimeContext
{
    public DateTime GetUtc() => DateTime.UtcNow;
}
