//using Microsoft.AspNetCore.Http;
//using Toolbox.Orleans.Types;
//using Toolbox.Types;

//namespace Toolbox.Orleans.Types;

//public static class ResultExtensions
//{
//    //public static IResult ToResult(this SpinResponse option) => option switch
//    //{
//    //    { StatusCode: StatusCode.OK } => Results.Ok(),
//    //    { StatusCode: StatusCode.BadRequest } v => Results.BadRequest(v.Error),
//    //    { StatusCode: StatusCode.NotFound } v => Results.NotFound(v.Error),
//    //    { StatusCode: StatusCode.Conflict } v => Results.Conflict(v.Error),

//    //    var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
//    //};
    
//    public static IResult ToResult(this Option option) => option switch
//    {
//        { StatusCode: StatusCode.OK } => Results.Ok(),
//        { StatusCode: StatusCode.BadRequest } v => Results.BadRequest(v.Error),
//        { StatusCode: StatusCode.NotFound } v => Results.NotFound(v.Error),
//        { StatusCode: StatusCode.Conflict } v => Results.Conflict(v.Error),

//        var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
//    };

//    //public static IResult ToResult<T>(this SpinResponse<T> option) => option switch
//    //{
//    //    { StatusCode: StatusCode.OK, HasValue: true } => Results.Ok(option.Return()),
//    //    { StatusCode: StatusCode.OK } => Results.Ok(new Option(StatusCode.OK)),
//    //    { StatusCode: StatusCode.BadRequest } v => Results.BadRequest(v.Error),
//    //    { StatusCode: StatusCode.NotFound } v => Results.NotFound(v.Error),
//    //    { StatusCode: StatusCode.Conflict } v => Results.Conflict(v.Error),

//    //    var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
//    //};

//    public static IResult ToResult<T>(this Option<T> option) => option switch
//    {
//        { StatusCode: StatusCode.OK, HasValue: true } => Results.Ok(option.Return()),
//        { StatusCode: StatusCode.OK } => Results.Ok(new Option(StatusCode.OK)),
//        { StatusCode: StatusCode.BadRequest } v => Results.BadRequest(v.Error),
//        { StatusCode: StatusCode.NotFound } v => Results.NotFound(v.Error),
//        { StatusCode: StatusCode.Conflict } v => Results.Conflict(v.Error),

//        var v => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
//    };
//}
