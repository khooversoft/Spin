using System.Net;

namespace Toolbox.Extensions;

public static class HttpStatusCodeExtensions
{
    public static bool IsOk(this HttpStatusCode subject) => subject == HttpStatusCode.OK;

    public static bool IsNoContent(this HttpStatusCode subject) => subject == HttpStatusCode.NoContent;

    public static bool IsOkAll(this HttpStatusCode subject) => subject switch
    {
        HttpStatusCode.OK => true,
        HttpStatusCode.NoContent => true,
        HttpStatusCode.Created => true,

        _ => false,
    };

    public static bool IsError(this HttpStatusCode subject) => !subject.IsOkAll();

    public static bool IsNotFound(this HttpStatusCode subject) => subject == HttpStatusCode.NotFound;
}
