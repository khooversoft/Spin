using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Client
{
    public abstract class ClientBase<T> where T : class
    {
        private readonly string _basePath;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ClientBase(HttpClient httpClient, string basePath, ILogger logger)
        {
            httpClient.VerifyNotNull(nameof(httpClient));
            basePath.VerifyNotEmpty(nameof(basePath));
            logger.VerifyNotNull(nameof(logger));

            _httpClient = httpClient;
            _basePath = basePath;
            _logger = logger;
        }

        public BatchSetCursor<string> List(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, $"api/{_basePath}/list", queryParameter, _logger);

        protected async Task<bool> Delete(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(path);
            _logger.LogTrace($"{nameof(Delete)}: path={path}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/{path}", token);
            _logger.LogTrace($"{nameof(Delete)}: response: {response.StatusCode} for path={path}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,

                _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
            };
        }

        protected async Task<T?> Get(string path, CancellationToken token)
        {
            path.VerifyNotEmpty(nameof(path));
            _logger.LogTrace($"{nameof(Get)}: path={path}");

            try
            {
                return await _httpClient.GetFromJsonAsync<T?>($"api/{path}", token);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Get: id={path} failed");
                return null;
            }
        }

        protected async Task Set(T subject, string path, CancellationToken token)
        {
            subject.VerifyNotNull(nameof(subject));
            path.VerifyNotEmpty(nameof(path));

            _logger.LogTrace($"{nameof(Set)}: path={path}");
            HttpResponseMessage message = await _httpClient.PostAsJsonAsync($"api/{_basePath}/{path}", subject, token);
            message.EnsureSuccessStatusCode();
        }
    }
}