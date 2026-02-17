//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Tools;

//namespace Toolbox.Store;

//public class SequenceSpaceProvider : IStoreSequenceProvider
//{
//    private readonly ILogger<SequenceSpaceProvider> _logger;
//    private readonly IServiceProvider _serviceProvider;

//    public SequenceSpaceProvider(string name, IServiceProvider serviceProvider, ILogger<SequenceSpaceProvider> logger)
//    {
//        Name = name.NotEmpty();
//        _serviceProvider = serviceProvider.NotNull();
//        _logger = logger.NotNull();
//    }

//    public string Name { get; }

//    public ISequenceStore<T> GetStore<T>(SpaceDefinition definition)
//    {
//        definition.SpaceFormat.Assert(x => x == SpaceFormat.Sequence, $"Invalid space format {definition.SpaceFormat} for list store");

//        var logSequenceNumber = _serviceProvider.GetRequiredService<LogSequenceNumber>();
//        SequenceKeySystem<T> sequenceKeySystem = new(definition.BasePath, logSequenceNumber);

//        var store = ActivatorUtilities.CreateInstance<SequenceSpace<T>>(_serviceProvider, sequenceKeySystem);
//        return store;
//    }
//}
