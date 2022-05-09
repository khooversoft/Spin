using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;

namespace Toolbox.Tools
{
    public class WorkAsync<TIn, TOut>
    {
        private readonly ActionBlock<TIn> _work;
        private readonly ConcurrentQueue<TOut> _saveQueue = new();

        public WorkAsync(Func<TIn, TOut> action, int maxDegreeOfParallelism)
        {
            action.NotNull(nameof(action));
            maxDegreeOfParallelism.Assert(x => x >= 1, x => $"MaxDegreeOfParallelism {maxDegreeOfParallelism} is invalid");

            var option = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            };

            _work = new ActionBlock<TIn>(LocalAction, option);

            void LocalAction(TIn value)
            {
                TOut result = action(value);
                _saveQueue.Enqueue(result);
            }
        }

        public WorkAsync<TIn, TOut> Post(TIn value) => this.Action(_ => _work.Post(value));

        public WorkAsync<TIn, TOut> Post(IEnumerable<TIn> values) => this.Action(_ => values.ForEach(x => Post(x)));

        public Task<IReadOnlyList<TOut>> Complete()
        {
            var tcs = new TaskCompletionSource<IReadOnlyList<TOut>>();

            _ = Task.Run(async () =>
            {
                await _work.Completion;
                tcs.SetResult(_saveQueue.ToArray());
            });

            _work.Complete();

            return tcs.Task;
        }
    }
}
