using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IChangeLog
{
    Guid LogKey { get; }
    Option Undo(GraphChangeContext graphContext);
}
