using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public record EdgeDelete : IChangeLog
{
    private readonly GraphEdge _oldValue;
    public EdgeDelete(GraphEdge oldValue) => _oldValue = oldValue.NotNull();

    public Guid LogKey { get; } = Guid.NewGuid();

    public Option Undo(GraphChangeContext graphContext)
    {
        graphContext.NotNull();

        graphContext.Map.Add(_oldValue);
        graphContext.Context.LogInformation("Rollback: restored edge logKey={logKey}, Edge key={key}, value={value}", LogKey, _oldValue.Key, _oldValue.ToJson());
        return StatusCode.OK;
    }
}
