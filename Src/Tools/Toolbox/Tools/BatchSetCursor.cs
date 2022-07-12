using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Model;

namespace Toolbox.Tools
{
    public class BatchSetCursor<T>
    {
        private static readonly BatchSet<T> _noResult = new BatchSet<T>();
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly string _postUrl;
        private readonly QueryParameter _queryParameter;
        private Func<CancellationToken, Task<BatchSet<T>>> _getFunc;

        public BatchSetCursor(HttpClient httpClient, string postUrl, QueryParameter queryParameter, ILogger logger)
        {
            _httpClient = httpClient;
            _queryParameter = queryParameter;
            _postUrl = postUrl;
            _logger = logger;

            _getFunc = Start;
        }

        public BatchSet<T>? Current { get; private set; }

        public async Task<BatchSet<T>> ReadNext(CancellationToken token = default) => await _getFunc(token);

        private async Task<BatchSet<T>> Continue(CancellationToken token)
        {
            var sl = _logger.LogEntryExit();
            _logger.LogTrace("Query={_queryParameter}", _queryParameter);

            QueryParameter queryParameter = _queryParameter with { Index = Current!.NextIndex };

            Current = await Post(queryParameter);

            _getFunc = Continue;
            return Current;
        }

        private async Task<BatchSet<T>> Post(QueryParameter queryParameter)
        {
            var sl = _logger.LogEntryExit();

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_postUrl, queryParameter);
            response.EnsureSuccessStatusCode();

            string content = (await response.Content.ReadAsStringAsync())
                .Assert(x => !x.IsEmpty(), "No content");

            _logger.LogTrace("Content={content}", content);

            return content.ToObject<BatchSet<T>>()
                .NotNull(name: "Serialization failed");
        }

        private async Task<BatchSet<T>> Start(CancellationToken token)
        {
            var sl = _logger.LogEntryExit();
            _logger.LogTrace("Query={_queryParameter}", _queryParameter);

            Current = await Post(_queryParameter);
            _getFunc = Continue;

            return Current;
        }
    }
}
