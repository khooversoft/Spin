//using Toolbox.Types;

//namespace Toolbox.Tools;

//public class QueueLock
//{
//    private readonly object _lockObject = new object();
//    private readonly Queue<TaskCompletionSource<bool>> _waitingQueue = new Queue<TaskCompletionSource<bool>>();
//    private bool _isLocked = false;

//    public QueueLock() { }

//    public Option AcquireLock(TimeSpan timeout)
//    {
//        var tcs = new TaskCompletionSource<bool>();

//        lock (_lockObject)
//        {
//            if (!_isLocked)
//            {
//                _isLocked = true;
//                return StatusCode.OK;
//            }

//            _waitingQueue.Enqueue(tcs);
//        }

//        try
//        {
//            var result = tcs.Task.WaitAsync(timeout).Result;
//            return result ? StatusCode.OK : (StatusCode.Conflict, "AcquireLock timed out");
//        }
//        catch
//        {
//            return (StatusCode.Conflict, "AcquireLock timed out");
//        }
//    }

//    public void ReleaseLock()
//    {
//        TaskCompletionSource<bool>? nextTcs = null;

//        lock (_lockObject)
//        {
//            if (_waitingQueue.Count > 0)
//            {
//                nextTcs = _waitingQueue.Dequeue();
//            }
//            else
//            {
//                _isLocked = false;
//            }
//        }

//        nextTcs?.SetResult(true);
//    }
//}

////file static class TaskExtensions
////{
////    public static async Task<bool> WaitAsync(this Task task, TimeSpan timeout)
////    {
////        var timeoutTask = Task.Delay(timeout);
////        var completedTask = await Task.WhenAny(task, timeoutTask);
////        return completedTask == task;
////    }
////}
