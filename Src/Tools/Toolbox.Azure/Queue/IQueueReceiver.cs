namespace Toolbox.Azure.Queue
{
    public interface IQueueReceiver
    {
        Task Start();
        Task Stop();
    }
}