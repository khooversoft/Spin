//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public class InMemoryFileAccess : IFileAccess
//{
//    private readonly InMemoryStoreControl _storeControl;
//    private readonly ILogger _logger;

//    internal InMemoryFileAccess(string path, InMemoryStoreControl storeControl, ILogger logger)
//    {
//        Path = path.NotEmpty();
//        _storeControl = storeControl.NotNull();
//        _logger = logger.NotNull();
//    }

//    public string Path { get; }

//    public Task<Option<string>> Add(DataETag data, ScopeContext context) => _storeControl.Add(Path, data, context.With(_logger));
//    public Task<Option> Append(DataETag data, ScopeContext context) => _storeControl.Append(Path, data, context.With(_logger));
//    public Task<Option> Delete(ScopeContext context) => _storeControl.Delete(Path, context.With(_logger));
//    public Task<Option> Exist(ScopeContext context) => _storeControl.Exist(Path, context.With(_logger));
//    public Task<Option<DataETag>> Get(ScopeContext context) => _storeControl.Get(Path, context.With(_logger));
//    public Task<Option<IStorePathDetail>> GetDetail(ScopeContext context) => _storeControl.GetDetail(Path, context.With(_logger));
//    public Task<Option<string>> Set(DataETag data, ScopeContext context) => _storeControl.Set(Path, data, context.With(_logger));
//}
