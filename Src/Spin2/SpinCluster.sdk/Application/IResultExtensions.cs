using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

public static class IResultExtensions
{
    public static IResult ToResult<T>(this Option<T> option) => option switch
    {
        { StatusCode: StatusCode.OK, HasValue: true } => Results.Ok(option.Return()),
        { StatusCode: StatusCode.OK } => Results.Ok(),
        { StatusCode: StatusCode.BadRequest } v => Results.BadRequest(v.Error),
        { StatusCode: StatusCode.NotFound } v => Results.NotFound(v.Error),
        { StatusCode: StatusCode.Conflict } v => Results.Conflict(v.Error),

        var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
    };
}
