using System;
using System.Threading.Tasks;

namespace Toolbox.Broker
{
    public interface IRoute
    {
        string Pattern { get; }

        Task SendToReceiver(object subject);
    }
}