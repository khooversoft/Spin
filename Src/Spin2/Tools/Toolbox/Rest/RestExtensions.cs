using System.Net;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Rest;

public static class RestExtensions
{
    public static async Task<Option<T>> GetContent<T>(this Task<RestResponse> httpResponse)
    {
        var response = await httpResponse;
        return response.GetContent<T>();
    }

    public static async Task<StatusCode> GetStatusCode(this Task<RestResponse> httpResponse)
    {
        var response = await httpResponse;
        return response.StatusCode.ToStatusCode();
    }

    public static async Task<Option> ToOption(this Task<RestResponse> httpResponse)
    {
        var response = await httpResponse;
        var content = response.GetContent<Option>();
        if (content.IsError()) return new Option(content.StatusCode, content.Error);

        return content.Return();
    }

    public static async Task<Option<T>> ToOption<T>(this Task<RestResponse> httpResponse)
    {
        var response = await httpResponse;
        var content = response.GetContent<Option<T>>();
        if (content.IsError()) return new Option<T>(content.StatusCode, content.Error);

        return content.Return();
    }

    public static Option<T> GetContent<T>(this RestResponse response)
    {
        if (response.StatusCode.IsError()) return new Option<T>(response.StatusCode.ToStatusCode(), response.Content);

        return response.Content switch
        {
            null => default,
            var v => tryDeserialize(v),
        };

        Option<T> tryDeserialize(string value)
        {
            try
            {
                return Json.Default.Deserialize<T>(value).NotNull();
            }
            catch (Exception ex)
            {
                response.Context.Location().LogCritical(ex, "Failed deserialization into type={type}, value={value}", typeof(T).Name, value);
                return new Option<T>(StatusCode.BadRequest);
            }
        };
    }

    public static async Task<Option<string>> GetContent(this HttpRequestMessage subject)
    {
        return subject.Content switch
        {
            null => Option<string>.None,
            not null => await subject.Content.ReadAsStringAsync(),
        };
    }

    public static bool IsOkayAll(this RestResponse subject) =>
        subject.StatusCode == HttpStatusCode.OK ||
        subject.StatusCode == HttpStatusCode.NoContent ||
        subject.StatusCode == HttpStatusCode.Created;

    public static bool IsOk(this RestResponse subject) => subject.StatusCode == HttpStatusCode.OK;
    public static bool IsNotFound(this RestResponse subject) => subject.StatusCode == HttpStatusCode.NotFound;
    public static bool IsError(this RestResponse subject) => !subject.IsOkayAll();
}
