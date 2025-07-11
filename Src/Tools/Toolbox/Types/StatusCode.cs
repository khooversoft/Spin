using System.Net;

namespace Toolbox.Types;

public enum StatusCode
{
    NoContent = 0,

    OK = 200,
    Created = 201,

    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    MethodNotAllowed = 405,
    Conflict = 409,
    Locked = 423,

    InternalServerError = 500,
    ServiceUnavailable = 503,
}


public static class OptionStatusCodeExtensions
{
    public static bool IsOk(this StatusCode subject) => subject == StatusCode.OK;
    public static bool IsNotFound(this StatusCode subject) => subject == StatusCode.NotFound;
    public static bool IsNoContent(this StatusCode subject) => subject == StatusCode.NoContent;
    public static bool IsConflict(this StatusCode subject) => subject == StatusCode.Conflict;
    public static bool IsBadRequest(this StatusCode subject) => subject == StatusCode.BadRequest;
    public static bool IsUnauthorized(this StatusCode subject) => subject == StatusCode.Unauthorized;
    public static bool IsForbidden(this StatusCode subject) => subject == StatusCode.Forbidden;
    public static bool IsLocked(this StatusCode subject) => subject == StatusCode.Locked;
    public static bool IsMethodNotAllowed(this StatusCode subject) => subject == StatusCode.MethodNotAllowed;

    public static bool IsSuccess(this StatusCode subject) => subject switch
    {
        StatusCode.OK => true,
        StatusCode.NoContent => true,
        StatusCode.Created => true,

        _ => false,
    };

    public static bool IsError(this StatusCode subject) => !subject.IsSuccess();


    public static StatusCode ToStatusCode(this HttpStatusCode subject) => subject switch
    {
        HttpStatusCode.NoContent => StatusCode.NoContent,
        HttpStatusCode.OK => StatusCode.OK,
        HttpStatusCode.Created => StatusCode.Created,
        HttpStatusCode.BadRequest => StatusCode.BadRequest,
        HttpStatusCode.Unauthorized => StatusCode.Unauthorized,
        HttpStatusCode.Forbidden => StatusCode.Forbidden,
        HttpStatusCode.NotFound => StatusCode.NotFound,
        HttpStatusCode.MethodNotAllowed => StatusCode.MethodNotAllowed,
        HttpStatusCode.Conflict => StatusCode.Conflict,
        HttpStatusCode.InternalServerError => StatusCode.InternalServerError,
        HttpStatusCode.ServiceUnavailable => StatusCode.ServiceUnavailable,
        HttpStatusCode.Locked => StatusCode.Locked,

        _ => StatusCode.BadRequest,
    };

    public static HttpStatusCode ToHttpStatusCode(this StatusCode subject) => subject switch
    {
        StatusCode.NoContent => HttpStatusCode.NoContent,
        StatusCode.OK => HttpStatusCode.OK,
        StatusCode.Created => HttpStatusCode.Created,
        StatusCode.BadRequest => HttpStatusCode.BadRequest,
        StatusCode.Unauthorized => HttpStatusCode.Unauthorized,
        StatusCode.Forbidden => HttpStatusCode.Forbidden,
        StatusCode.NotFound => HttpStatusCode.NotFound,
        StatusCode.MethodNotAllowed => HttpStatusCode.MethodNotAllowed,
        StatusCode.Conflict => HttpStatusCode.Conflict,
        StatusCode.InternalServerError => HttpStatusCode.InternalServerError,
        StatusCode.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
        StatusCode.Locked => HttpStatusCode.Locked,

        _ => HttpStatusCode.BadRequest,
    };
}
