using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public interface IQueueReceiver
    {
        void Start();
        Task Stop();
    }
}