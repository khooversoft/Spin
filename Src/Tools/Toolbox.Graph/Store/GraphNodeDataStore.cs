//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Types;

//namespace Toolbox.Graph.Store;

//internal class GraphNodeDataStore : IGraphStore
//{
//    public GraphNodeDataStore(IGraphClient graphClient)
//    {
//        graphClient.VerifyNotNull(nameof(graphClient));

//        GraphClient = graphClient;
//    }
//    public Task<Option<string>> Add(string nodeKey, string name, DataETag data, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option> Delete(string nodeKey, string name, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option> Exist(string nodeKey, string name, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option<DataETag>> Get(string nodeKey, string name, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option<string>> Set(string nodeKey, string name, DataETag data, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }
//}
