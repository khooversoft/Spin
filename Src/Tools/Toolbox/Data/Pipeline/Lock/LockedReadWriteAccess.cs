//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;


//public partial class LockManager
//{
//    public class LockedReadWriteAccess : IFileReadWriteAccess
//    {
//        private readonly LockManager _manager;
//        private readonly string _path;
//        private readonly LockMode _lockMode;
//        private IFileReadWriteAccess _access;

//        internal LockedReadWriteAccess(LockManager manager, string path, LockMode lockMode, IFileReadWriteAccess access)
//        {
//            _manager = manager;
//            _path = path;
//            _lockMode = lockMode;
//            _access = access;
//        }

//        public string Path => _access.Path;

//        public async Task<Option<string>> Append(DataETag data, ScopeContext context)
//        {
//            var result = await Retry(async () => await _access.Append(data, context), context);
//            if( result.IsError()) return result.ToOptionStatus<string>();

//            string value = result.Return() switch
//            {
//                string v => v,
//                _ => throw new InvalidOperationException("Unexpected return type from Append operation"),
//            };

//            return value;
//        }

//        public async Task<Option<DataETag>> Get(ScopeContext context)
//        {
//            var result = await Retry(async () => await _access.Get(context), context);
//            if (result.IsError()) return result.ToOptionStatus<DataETag>();

//            DataETag value = result.Return() switch
//            {
//                DataETag v => v,
//                Option<DataETag> v => v.Return(),
//                _ => throw new InvalidOperationException("Unexpected return type from Get operation"),
//            };

//            return value;
//        }

//        public async Task<Option<string>> Set(DataETag data, ScopeContext context)
//        {
//            var result = await Retry(async () => await _access.Set(data, context), context);
//            if (result.IsError()) return result.ToOptionStatus<string>();

//            string value = result.Return() switch
//            {
//                string v => v,
//                Option<string> v => v.Return(),
//                _ => throw new InvalidOperationException("Unexpected return type from Set operation"),
//            };

//            return value;
//        }

//        private async Task<Option<object>> Retry(Func<Task<Option<object>>> execute, ScopeContext context)
//        {
//            int retry = 0;

//            while (retry++ < 2)
//            {
//                var result = await execute();

//                if (result.IsOk()) return result;
//                if (result.IsLocked())
//                {
//                    await ResetLock(context);
//                    continue;
//                }

//                return result;
//            }

//            return StatusCode.Conflict;
//        }

//        private async Task<Option> ResetLock(ScopeContext context)
//        {
//            context.LogDebug("Append operation is locked, path={path}", _access.Path);
//            var retryLockOption = await _manager.ProcessLock(_path, _lockMode, context);
//            if (retryLockOption.IsError()) return retryLockOption;

//            _access = _manager.Lookup(_path, context)
//                .NotNull("Failed to register lock")
//                .FileLeasedAccess.NotNull("Failed to get file leased access");

//            return StatusCode.OK;
//        }
//    }
//}