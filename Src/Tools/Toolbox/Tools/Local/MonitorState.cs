using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Tools.Local
{
    public enum MonitorState
    {
        Stopped = 0,
        Started,
        Running,
        Restart,
        Failed,
    }
}
