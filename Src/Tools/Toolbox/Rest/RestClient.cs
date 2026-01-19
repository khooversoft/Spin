using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Rest;


public enum RestContentType
{
    Json,
    Xml11,
    Xml12
}

/// <summary>
/// REST Client provides a builder pattern for making a REST API call.
/// 
/// Note: if the Absolute URI is specified, this URI is used and not build
/// process is not used.
/// </summary>
public class RestClient
{
    private readonly HttpClient _client = null!;
    private readonly Dictionary<string, string> _headers = new();

    public RestClient(HttpClient client) => _client = client.NotNull();

    public string? Path { get; private set; }
    public object? Content { get; private set; }
    public RestContentType ContentType { get; private set; }

    public bool EnsureSuccessStatusCode { get; private set; } = false;
    public IReadOnlyDictionary<string, string> Headers => _headers;

    public RestClient Clear()
    {
        Path = null;
        Content = null;
        ContentType = RestContentType.Json;

        return this;
    }

    public RestClient SetPath(string path) => this.Action(x => x.Path = path);
    public RestClient SetContent(HttpContent content) => this.Action(x => Content = content);
    public RestClient SetContent<T>(T value, bool required = true, RestContentType contentType = RestContentType.Json)
    {
        if (required) value.NotNull();

        Content = value;
        ContentType = contentType;

        return this;
    }

    public RestClient SetEnsureSuccessStatusCode(bool state) => this.Action(x => x.EnsureSuccessStatusCode = state);

    public RestClient AddHeader(string header, string? value)
    {
        if (value.IsNotEmpty()) _headers[header] = value!;
        return this;
    }

    public async Task<RestResponse> SendAsync(HttpRequestMessage requestMessage, ILogger logger, CancellationToken cancellationToken = default)
    {
        string? requestPayload = (await requestMessage.GetContent()).Return(false);

        logger.LogDebug(
            "[RestClient-Calling] {uri}, method={method}, request={request}",
            requestMessage.RequestUri?.ToString() ?? "<no uri>",
            requestMessage.Method,
            requestPayload
            );

        requestPayload = requestPayload switch
        {
            null => null,
            { Length: <= 100 } => requestPayload,
            _ => requestPayload.Truncate(100, addEllipse: true),
        };

        try
        {
            HttpResponseMessage response;
            string content;

            response = await _client.SendAsync(requestMessage.NotNull(), cancellationToken);
            content = await response.Content.ReadAsStringAsync();

            logger.Log(
                setLogLevel(response),
                "[RestClient-Response] from {uri}, method={method}, StatusCode={statusCode}, request={request}, response={response}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                response.StatusCode,
                requestPayload,
                (content.ToNullIfEmpty() ?? "<no content>").Truncate(100, true)
                );

            logger.LogTrace(
                "[RestClient-Response] from {uri}, method={method}, StatusCode={statusCode}, response={response}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                response.StatusCode,
                (content.ToNullIfEmpty() ?? "<no content>")
                );

            var result = new RestResponse
            {
                StatusCode = response.StatusCode,
                Content = content,
                Logger = logger
            };

            if (EnsureSuccessStatusCode) response.EnsureSuccessStatusCode();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError("[RestClient-Error] call to {uri} failed, method={method}, request={request}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                requestPayload
                );

            var result = new RestResponse
            {
                StatusCode = StatusCode.InternalServerError.ToHttpStatusCode(),
                Content = ex.ToString(),
                Logger = logger
            };

            return result;
        }

        LogLevel setLogLevel(HttpResponseMessage response) => response.StatusCode switch
        {
            HttpStatusCode.OK => LogLevel.Debug,
            HttpStatusCode.NoContent => LogLevel.Debug,
            HttpStatusCode.Created => LogLevel.Debug,
            HttpStatusCode.Accepted => LogLevel.Debug,
            HttpStatusCode.NotFound => LogLevel.Debug,

            _ => LogLevel.Error,
        };
    }

    public Task<RestResponse> GetAsync(ILogger logger, CancellationToken cancellationToken = default) => SendAsync(BuildRequestMessage(HttpMethod.Get), logger, cancellationToken);
    public Task<RestResponse> DeleteAsync(ILogger logger, CancellationToken cancellationToken = default) => SendAsync(BuildRequestMessage(HttpMethod.Delete), logger, cancellationToken);
    public Task<RestResponse> PostAsync(ILogger logger, CancellationToken cancellationToken = default) => SendAsync(BuildRequestMessage(HttpMethod.Post), logger, cancellationToken);
    public Task<RestResponse> PutAsync(ILogger logger, CancellationToken cancellationToken = default) => SendAsync(BuildRequestMessage(HttpMethod.Put), logger, cancellationToken);
    public Task<RestResponse> PatchAsync(ILogger logger, CancellationToken cancellationToken = default) => SendAsync(BuildRequestMessage(HttpMethod.Patch), logger, cancellationToken);

    private HttpRequestMessage BuildRequestMessage(HttpMethod method)
    {
        var request = new HttpRequestMessage(method, Path)
        {
            Content = Content switch
            {
                null => null,
                HttpContent v => v,

                string v when ContentType == RestContentType.Xml11 => new StringContent(v, Encoding.UTF8, "text/xml"),
                string v when ContentType == RestContentType.Xml12 => new StringContent(v, Encoding.UTF8, "application/soap+xml"),

                string v => new StringContent(v, Encoding.UTF8, "text/plain"),

                var v => new StringContent(Json.Default.SerializePascal(v), Encoding.UTF8, "application/json"),
            },
        };

        _headers.ForEach(x => request.Headers.Add(x.Key, x.Value));
        return request;
    }
}
