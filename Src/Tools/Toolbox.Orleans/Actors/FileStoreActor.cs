//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Orleans.Runtime;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Orleans;

//public interface IFileStoreActor : IGrainWithStringKey
//{
//    Task<Option<string>> Add(DataETag data, string traceId);
//    Task<Option> Exist(string traceId);
//    Task<Option> Delete(string traceId);
//    Task<Option<DataETag>> Get(string traceId);
//    Task<Option<string>> Set(DataETag data, string traceId);
//}

//public class FileStoreActor : Grain, IFileStoreActor
//{
//    private readonly ILogger<DirectoryActor> _logger;
//    private readonly ActorCacheState<DataETag> _state;

//    public FileStoreActor(
//        [PersistentState("json", OrleansConstants.StorageProviderName)] IPersistentState<DataETag> state,
//        ILogger<DirectoryActor> logger
//        )
//    {
//        _state = new ActorCacheState<DataETag>(state, TimeSpan.FromMinutes(15));
//        _logger = logger.NotNull();
//    }

//    public override async Task OnActivateAsync(CancellationToken cancellationToken)
//    {
//        this.GetPrimaryKeyString().Assert(x => FileStoreTool.IsPathValid(x), x => $"ActorId={x} is not a valid path");
//        await base.OnActivateAsync(cancellationToken);
//    }

//}