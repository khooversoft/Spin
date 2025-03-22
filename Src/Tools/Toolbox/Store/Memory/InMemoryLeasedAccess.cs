//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public class InMemoryLeasedAccess : IFileLeasedAccess
//{
//    private readonly InMemoryStoreControl _storeControl;
//    private readonly InMemoryStoreLeaseControl _leaseControl;
//    private readonly LeaseRecord _leaseRecord;

//    internal InMemoryLeasedAccess(LeaseRecord leaseRecord, InMemoryStoreControl storeControl, InMemoryStoreLeaseControl leaseControl)
//    {
//        _leaseRecord = leaseRecord.NotNull();
//        _storeControl = storeControl.NotNull();
//        _leaseControl = leaseControl.NotNull();
//    }

//    public string Path => _leaseRecord.Path;
//    public string LeaseId => _leaseRecord.LeaseId;
//    public DateTimeOffset Expiration => _leaseRecord.Expiration;

//    public Task<Option> Append(DataETag data, ScopeContext context) => _storeControl.Append(_path, data, context);


//    public Task<Option<DataETag>> Get(ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option> Release(ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option> Renew(ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Option<string>> Set(DataETag data, ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
//}
