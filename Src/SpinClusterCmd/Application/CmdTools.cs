using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterCmd.Application;

internal static class CmdTools
{
    public static Option<T> LoadJson<T>(string jsonFile, IValidator<T> validator, ScopeContext context)
    {
        jsonFile.NotEmpty();
        validator.NotNull();

        context.LogInformation("Processing file {file}", jsonFile);

        if (!File.Exists(jsonFile))
        {
            context.LogError("File {file} does not exist", jsonFile);
            return StatusCode.NotFound;
        }

        string json = File.ReadAllText(jsonFile);
        T? obj = json.ToObject<T>();
        if (obj == null)
        {
            context.LogError("Cannot parse {file}", jsonFile);
            return StatusCode.BadRequest;
        }

        var v = validator.Validate(obj);
        if (v.IsError())
        {
            context.LogError("Option is not valid, error={error}", v.Error);
            return v.ToOptionStatus<T>();
        }

        return obj;
    }
}
