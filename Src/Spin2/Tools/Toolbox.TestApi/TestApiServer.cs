using System.Net;
using Toolbox.Types;

namespace Toolbox.TestApi;

public class TestApiServer
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        // Normal API calls
        app.MapGet("/hello", () => "hello");
        app.MapGet("/helloWithError", () => Results.BadRequest("badRequest for hello"));

        app.MapGet("/statusCodeOnlyCall", () => Results.StatusCode((int)HttpStatusCode.Conflict));

        app.MapGet("/justOk", () => Results.Ok());
        app.MapGet("/justOkWithModel", () => Results.Ok(ModelDefaults.TestModel));
        app.MapGet("/justOkWithMessage", () => Results.Ok("this works"));

        // Option calls
        app.MapGet("/option", () => new Option(StatusCode.OK));
        app.MapGet("/optionWithError", () => new Option(StatusCode.BadRequest, ModelDefaults.BadRequestResponse));

        app.MapGet("/testModel", () => ModelDefaults.TestModel);
        app.MapGet("/testModelWithError", () => Results.BadRequest(ModelDefaults.BadRequestResponse));
        app.MapGet("/testModelBadRequestNoErrorMessage", () => Results.BadRequest());

        // Option T calls
        app.MapGet("/option_t", () => new Option<TestModel>(ModelDefaults.TestModel));
        app.MapGet("/option_t_withError", () => Results.BadRequest(ModelDefaults.BadRequestResponse));
        app.MapGet("/option_t_withInternalError", () => new Option<TestModel>(StatusCode.BadRequest, ModelDefaults.BadRequestResponse));

        app.Run();
    }
}

public record TestModel(string Name, int Value, DateTime Date);

public static class ModelDefaults
{
    public const string BadRequestResponse = "badRequest";

    public static TestModel TestModel { get; } = new TestModel("name1", 20, DateTime.Now);
}

