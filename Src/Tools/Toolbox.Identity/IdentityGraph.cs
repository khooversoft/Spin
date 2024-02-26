using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Identity;

public class IdentityGraph
{
    private GraphMap _map;

    public IdentityGraph() : this(new GraphMap()) { }
    public IdentityGraph(GraphMap map)
    {
        _map = map.NotNull();
        User = new UserAccess(_map);
    }

    public UserAccess User { get; }
}
