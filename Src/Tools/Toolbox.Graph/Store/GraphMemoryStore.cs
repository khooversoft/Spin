//using System.Collections.Concurrent;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph;

//public class GraphMemoryStore : IGraphStore
//{
//    private ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

//    public Task<Option> Add(string fileId, string json, ScopeContext context)
//    {
//        Option option = _store.TryAdd(fileId, json) switch
//        {
//            true => StatusCode.OK,
//            false => (StatusCode.Conflict, $"FileId={fileId} already exist"),
//        };

//        return option.ToTaskResult();
//    }

//    public Task<Option> Add<T>(string fileId, T value, ScopeContext context) where T : class
//    {
//        string json = value.ToJson();
//        return Add(fileId, json, context);
//    }

//    public Task<Option> Delete(string fileId, ScopeContext context)
//    {
//        Option option = _store.TryRemove(fileId, out var _) switch
//        {
//            true => StatusCode.OK,
//            false => (StatusCode.NotFound, $"FileId={fileId} not found"),
//        };

//        return option.ToTaskResult();
//    }

//    public Task<Option> Exist(string fileId, ScopeContext context)
//    {
//        Option option = _store.ContainsKey(fileId) switch
//        {
//            true => StatusCode.OK,
//            false => (StatusCode.NotFound, $"FileId={fileId} not found"),
//        };

//        return option.ToTaskResult();
//    }

//    public Task<Option<string>> Get(string fileId, ScopeContext context)
//    {
//        Option<string> option = _store.TryGetValue(fileId, out var value) switch
//        {
//            true => value,
//            false => (StatusCode.NotFound, $"FileId={fileId} not found"),
//        };

//        return option.ToTaskResult();
//    }

//    public async Task<Option<T>> Get<T>(string fileId, ScopeContext context)
//    {
//        var option = Get(fileId, context);
//        if( option.IsEr
//    }

//    public Task<Option> Set(string fileId, string json, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option> Set<T>(string fileId, T value, ScopeContext context) where T : class
//    {
//        throw new NotImplementedException();
//    }

//    //public Task<Option> Add<T>(string nodeKey, T node, ScopeContext context) where T : class
//    //{
//    //    Option option = _store.TryAdd(nodeKey, node) switch
//    //    {
//    //        true => StatusCode.OK,
//    //        false => (StatusCode.Conflict, $"Node key={nodeKey} already exist"),
//    //    };

//    //    return option.ToTaskResult();

//    //}

//    //public Task<Option> Delete(string nodeKey, ScopeContext context)
//    //{
//    //    Option option = _store.TryRemove(nodeKey, out var _) switch
//    //    {
//    //        true => StatusCode.OK,
//    //        false => (StatusCode.NotFound, $"Node key={nodeKey} not found"),
//    //    };

//    //    return option.ToTaskResult();
//    //}

//    //public Task<Option> Exist(string nodeKey, ScopeContext context)
//    //{
//    //    Option option = _store.ContainsKey(nodeKey) switch
//    //    {
//    //        true => StatusCode.OK,
//    //        false => StatusCode.NotFound,
//    //    };

//    //    return option.ToTaskResult();
//    //}

//    //public Task<Option<T>> Get<T>(string nodeKey, ScopeContext context)
//    //{
//    //    Option<T> option = _store.TryGetValue(nodeKey, out var value) switch
//    //    {
//    //        true => (T)value,
//    //        false => StatusCode.NotFound,
//    //    };

//    //    return option.ToTaskResult();
//    //}

//    //public Task<Option> Set<T>(string nodeKey, T node, ScopeContext context) where T : class
//    //{
//    //    _store[nodeKey.NotEmpty()] = node;
//    //    return new Option(StatusCode.OK).ToTaskResult();
//    //}
//}
