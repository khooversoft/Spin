using System.Net;

namespace Toolbox.Types.Maybe;

public enum OptionStatus
{
    NoContent = 0,

    OK = 200,
    Created = 201,

    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,

    InternalServerError = 500,
    ServiceUnavailable = 503,
}


public static class OptionStatusCodeExtensions
{
    public static bool IsOk(this OptionStatus subject) => subject == OptionStatus.OK;

    public static bool IsNoContent(this OptionStatus subject) => subject == OptionStatus.NoContent;

    public static bool IsOkAll(this OptionStatus subject) => subject switch
    {
        OptionStatus.OK => true,
        OptionStatus.NoContent => true,
        OptionStatus.Created => true,

        _ => false,
    };

    public static bool IsError(this OptionStatus subject) => !subject.IsOkAll();

    public static bool IsNotFound(this OptionStatus subject) => subject == OptionStatus.NotFound;

    public static OptionStatus ToOptionStatusCode(this HttpStatusCode subject) => subject switch
    {
        HttpStatusCode.NoContent => OptionStatus.NoContent,
        HttpStatusCode.OK => OptionStatus.OK,
        HttpStatusCode.Created => OptionStatus.Created,
        HttpStatusCode.BadRequest => OptionStatus.BadRequest,
        HttpStatusCode.Unauthorized => OptionStatus.Unauthorized,
        HttpStatusCode.Forbidden => OptionStatus.Forbidden,
        HttpStatusCode.NotFound => OptionStatus.NotFound,
        HttpStatusCode.Conflict => OptionStatus.Conflict,
        HttpStatusCode.InternalServerError => OptionStatus.InternalServerError,
        HttpStatusCode.ServiceUnavailable => OptionStatus.ServiceUnavailable,

        _ => OptionStatus.BadRequest,
    };

    public static HttpStatusCode ToHttpStatusCode(this OptionStatus subject) => subject switch
    {
        OptionStatus.NoContent => HttpStatusCode.NoContent,
        OptionStatus.OK => HttpStatusCode.OK,
        OptionStatus.Created => HttpStatusCode.Created,
        OptionStatus.BadRequest => HttpStatusCode.BadRequest,
        OptionStatus.Unauthorized => HttpStatusCode.Unauthorized,
        OptionStatus.Forbidden => HttpStatusCode.Forbidden,
        OptionStatus.NotFound => HttpStatusCode.NotFound,
        OptionStatus.Conflict => HttpStatusCode.Conflict,
        OptionStatus.InternalServerError => HttpStatusCode.InternalServerError,
        OptionStatus.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,

        _ => HttpStatusCode.BadRequest,
    };
}
