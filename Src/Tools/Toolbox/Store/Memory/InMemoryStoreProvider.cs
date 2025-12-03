//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Store;

//public class InMemoryStoreProvider : IStoreFileProvider
//{
//    private readonly ILogger<InMemoryStoreProvider> _logger;
//    private readonly MemoryStore _memoryStore;

//    public InMemoryStoreProvider(MemoryStore memoryStore, ILogger<InMemoryStoreProvider> logger)
//    {
//        _logger = logger.NotNull();
//        _memoryStore = memoryStore.NotNull();
//    }

//    public string Name => "InMemoryStoreProvider";

//}
