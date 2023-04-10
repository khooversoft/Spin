using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Toolbox.Tools.Local
{
    public class MonitorStateScope
    {
        private int _monitorState;

        public MonitorStateScope(MonitorState monitorState) => _monitorState = (int)monitorState;

        public MonitorState Current => (MonitorState)_monitorState;

        public bool IsRunning => _monitorState switch
        {
            (int)MonitorState.Started => true,
            (int)MonitorState.Running => true,

            _ => false
        };

        public void Set(MonitorState monitorState) => Interlocked.Exchange(ref _monitorState, (int)monitorState);
    }
}
