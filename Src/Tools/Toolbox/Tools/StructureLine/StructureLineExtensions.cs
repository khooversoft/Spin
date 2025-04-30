using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class StructureLineExtensions
{
    public static StructureLineBuilder Add(this StructureLineBuilder subject, ScopeContext context) => subject.Add("traceId={traceId}", context.TraceId.NotEmpty());

    public static StructureLineBuilder Add(this StructureLineBuilder subject, Option option) =>
        subject.NotNull().Add("statusCode={statusCode}, error={error}", option.StatusCode, option.Error ?? "< no error >");

    public static StructureLineBuilder Add(this StructureLineBuilder subject, CodeLocation codeLocation)
    {
        subject.Add("callerFunction={callerFunction}", codeLocation.CallerFunction);
        subject.Add("callerFilePath={callerFilePath}", codeLocation.CallerFilePath);
        subject.Add("callerLineNumber={callerLineNumber}", codeLocation.CallerLineNumber.ToString());
        if (codeLocation.ArgumentName.IsNotEmpty()) subject.Add("argumentName={argumentName}", codeLocation.ArgumentName);

        return subject;
    }

    public static StructureLineBuilder Add(this StructureLineBuilder subject, ScopeContextLocation context)
    {
        subject.Add(context.Context);
        subject.Add(context.Location);
        return subject;
    }
}
