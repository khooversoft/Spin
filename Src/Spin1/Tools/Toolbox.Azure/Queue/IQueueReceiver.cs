using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public interface IQueueReceiver
    {
        Task Start();
        Task Stop();
    }
}