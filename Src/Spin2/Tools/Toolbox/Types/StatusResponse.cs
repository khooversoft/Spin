using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;

namespace Toolbox.Types;

public readonly struct StatusResponse
{
    public StatusResponse(StatusCode statusCode) => StatusCode = statusCode;
    public StatusResponse(StatusCode statusCode, string? error) => (StatusCode, Error) = (statusCode, error);

    public StatusCode StatusCode { get; init; }
    public string? Error { get; init; }
}


public static class StatusResponseExtensions
{
    public static StatusResponse ToStatusResponse<T>(this Option<T> subject) => new StatusResponse(subject.StatusCode, subject.Error);

    public static StatusResponse ToStatusResponse(this ValidatorResult subject) => new StatusResponse(
        subject.IsValid ? StatusCode.OK : StatusCode.BadRequest,
        subject.FormatErrors()
        );
}