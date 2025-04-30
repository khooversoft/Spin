using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class StructureLineExtensions
{
    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, ScopeContext context)
    {
        var newSubject = subject.Add("traceId={traceId}", context.TraceId.NotEmpty());
        return newSubject;
    }

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, Option option)
    {
        var newSubject = subject
            .Add("statusCode={statusCode}", option.StatusCode)
            .Add("error={error}", option.Error ?? "< no error >");

        return newSubject;
    }

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, CodeLocation codeLocation)
    {
        var newSubject = subject
            .Add("callerFunction={callerFunction}", codeLocation.CallerFunction)
            .Add("callerFilePath={callerFilePath}", codeLocation.CallerFilePath)
            .Add("callerLineNumber={callerLineNumber}", codeLocation.CallerLineNumber.ToString())
            .Func(x => codeLocation.ArgumentName.IsNotEmpty() ? x.Add("argumentName={argumentName}", codeLocation.ArgumentName) : x);

        return newSubject;
    }

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, ScopeContextLocation context)
    {
        var newSubject = subject
            .Add(context.Context)
            .Add(context.Location);

        return newSubject;
    }

    public static IEnumerable<StructureLineRecord> Add(this IEnumerable<StructureLineRecord> subject, ILoggingContext context)
    {
        var newSubject = context switch
        {
            ScopeContext v => subject.Add(v),
            ScopeContextLocation v => subject.Add(v.Context).Add(v.Location),
            _ => throw new UnreachableException(),
        };

        return newSubject;
    }
}
