using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Graph;

public class GraphDb
{
    public GraphDb(IGraphStore graphStore)
    {
        Graph = new GraphAccess(graphStore);
        Store = new GraphStoreAccess(graphStore, Graph);
    }

    public GraphAccess Graph { get; }
    public GraphStoreAccess Store { get; }
}
