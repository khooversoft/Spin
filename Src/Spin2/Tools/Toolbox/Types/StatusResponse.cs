using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types;

public readonly struct StatusResponse
{
    public StatusResponse(StatusCode statusCode) => StatusCode = statusCode;
    public StatusResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);

    public StatusCode StatusCode { get; }
    public string? Error { get; }
}


public static class StatusResponseExtensions
{
    public static StatusResponse ToStatusReponse<T>(this Option<T> subject) => new StatusResponse(subject.StatusCode, subject.Error);
}