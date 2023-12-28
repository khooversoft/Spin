﻿using System.Net;
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

    public async Task<RestResponse> SendAsync(HttpRequestMessage requestMessage, ScopeContext context)
    {
        string? requestPayload = (await requestMessage.GetContent()).Return(false);

        context.Location().LogTrace(
            "[RestClient-Calling] {uri}, method={method}, request={request}",
            requestMessage.RequestUri?.ToString() ?? "<no uri>",
            requestMessage.Method,
            requestPayload
            );

        requestPayload = requestPayload switch
        {
            null => null,
            { Length: <= 100 } => requestPayload,
            _ => requestPayload.Truncate(100) + "...",
        };

        try
        {
            HttpResponseMessage response;
            string content;

            response = await _client.SendAsync(requestMessage.NotNull(), context);
            content = await response.Content.ReadAsStringAsync();

            context.Location().Log(
                setLogLevel(response),
                "[Restclient-Response] from {uri}, method={method}, StatusCode={statusCode}, request={request}, response={response}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                response.StatusCode,
                requestPayload,
                (content.ToNullIfEmpty() ?? "<no content>").Truncate(100)
                );

            context.Location().LogTrace(
                "[Restclient-Response] from {uri}, method={method}, StatusCode={statusCode}, response={response}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                response.StatusCode,
                (content.ToNullIfEmpty() ?? "<no content>")
                );

            var result = new RestResponse
            {
                StatusCode = response.StatusCode,
                Content = content,
                Context = context
            };

            if (EnsureSuccessStatusCode) response.EnsureSuccessStatusCode();
            return result;
        }
        catch (Exception ex)
        {
            context.Location().LogCritical("[Restclient-Error] call to {uri} failed, method={method}, request={request}",
                requestMessage.RequestUri?.ToString(),
                requestMessage.Method,
                requestPayload
                );

            var result = new RestResponse
            {
                StatusCode = StatusCode.InternalServerError.ToHttpStatusCode(),
                Content = ex.ToString(),
                Context = context
            };

            return result;
        }

        LogLevel setLogLevel(HttpResponseMessage response) => response.StatusCode switch
        {
            HttpStatusCode.OK => LogLevel.Information,
            HttpStatusCode.NoContent => LogLevel.Information,
            HttpStatusCode.Created => LogLevel.Information,
            HttpStatusCode.Accepted => LogLevel.Information,

            HttpStatusCode.NotFound => LogLevel.Warning,

            _ => LogLevel.Error,
        };
    }

    public Task<RestResponse> GetAsync(ScopeContext context) => SendAsync(BuildRequestMessage(HttpMethod.Get), context);
    public Task<RestResponse> DeleteAsync(ScopeContext context) => SendAsync(BuildRequestMessage(HttpMethod.Delete), context);
    public Task<RestResponse> PostAsync(ScopeContext context) => SendAsync(BuildRequestMessage(HttpMethod.Post), context);
    public Task<RestResponse> PutAsync(ScopeContext context) => SendAsync(BuildRequestMessage(HttpMethod.Put), context);
    public Task<RestResponse> PatchAsync(ScopeContext context) => SendAsync(BuildRequestMessage(HttpMethod.Patch), context);

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
