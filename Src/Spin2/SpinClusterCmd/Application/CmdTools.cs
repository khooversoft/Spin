using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterCmd.Application;

internal static class CmdTools
{
    public static Option<T> LoadJson<T>(string jsonFile, IValidator<T> validator, ScopeContext context)
    {
        jsonFile.NotEmpty();
        validator.NotNull();

        context.Trace().LogInformation("Processing file {file}", jsonFile);

        if (!File.Exists(jsonFile))
        {
            context.Trace().LogError("File {file} does not exist", jsonFile);
            return StatusCode.NotFound;
        }

        string json = File.ReadAllText(jsonFile);
        T? obj = json.ToObject<T>();
        if (obj == null)
        {
            context.Trace().LogError("Cannot parse {file}", jsonFile);
            return StatusCode.BadRequest;
        }
        var v = validator.Validate(obj);
        if (v.IsError())
        {
            v.LogResult(context.Location());
            context.Trace().LogError("Option is not valid, error={error}", v.Error);
            return v.ToOptionStatus<T>();
        }

        return obj;
    }
}
