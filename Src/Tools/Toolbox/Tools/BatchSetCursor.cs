using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Model;

namespace Toolbox.Tools
{
    public class BatchSetCursor<T>
    {
        private readonly ILogger _logger;
        private readonly Func<QueryParameter, Task<BatchSet<T>>> _post;
        private readonly QueryParameter _queryParameter;
        private Func<CancellationToken, Task<BatchSet<T>>> _getFunc;

        public BatchSetCursor(Func<QueryParameter, Task<BatchSet<T>>> post, QueryParameter queryParameter, ILogger logger)
        {
            post.VerifyNotNull(nameof(post));
            queryParameter.VerifyNotNull(nameof(queryParameter));
            logger.VerifyNotNull(nameof(logger));

            _post = post;
            _queryParameter = queryParameter;
            _logger = logger;

            _getFunc = Start;
        }

        public BatchSet<T>? Current { get; private set; }

        public async Task<BatchSet<T>> ReadNext(CancellationToken token = default) => await _getFunc(token);

        private async Task<BatchSet<T>> Continue(CancellationToken token)
        {
            _logger.LogTrace($"{nameof(Continue)}: Query={_queryParameter}");

            QueryParameter queryParameter = _queryParameter with { Index = Current!.NextIndex };
            Current = await _post(queryParameter);

            return Current;
        }

        private async Task<BatchSet<T>> Start(CancellationToken token)
        {
            _logger.LogTrace($"{nameof(Start)}: Query={_queryParameter}");

            Current = await _post(_queryParameter);
            _getFunc = Continue;

            return Current;
        }
    }
}
