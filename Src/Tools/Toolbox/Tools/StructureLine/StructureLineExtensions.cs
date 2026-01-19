using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class StructureLineExtensions
{
    public static StructureLineBuilder Add(this StructureLineBuilder subject, Option option)
    {
        var newSubject = subject.Add("statusCode={statusCode}", option.StatusCode);
        if (option.Error.IsNotEmpty()) subject.Add("error={error}", option.Error);

        return newSubject;
    }

    public static StructureLineBuilder Add(this StructureLineBuilder subject, CodeLocation codeLocation)
    {
        var newSubject = subject
            .Add("callerFunction={callerFunction}", codeLocation.CallerFunction)
            .Add("callerFilePath={callerFilePath}", codeLocation.CallerFilePath)
            .Add("callerLineNumber={callerLineNumber}", codeLocation.CallerLineNumber.ToString())
            .Func(x => codeLocation.ArgumentName.IsNotEmpty() ? x.Add("argumentName={argumentName}", codeLocation.ArgumentName) : x);

        return newSubject;
    }
}
