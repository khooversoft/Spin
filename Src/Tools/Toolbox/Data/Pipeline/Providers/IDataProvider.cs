//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;


//public interface IDataProvider
//{
//    IDataProvider? InnerHandler { get; set; }
//    Task<Option<DataPipelineContext>> Execute(DataPipelineContext dataContext, ScopeContext context);

//    Task<Option<DataPipelineContext>> NextExecute(DataPipelineContext dataContext, ScopeContext context) => InnerHandler switch
//    {
//        null => dataContext.NotNull().Command switch
//        {
//            DataPipelineCommand.Append => ReturnOK(dataContext),
//            DataPipelineCommand.Delete => ReturnOK(dataContext),
//            DataPipelineCommand.Drain => ReturnOK(dataContext),
//            DataPipelineCommand.Get => ReturnNotFound(dataContext),
//            DataPipelineCommand.Set => ReturnOK(dataContext),
//            DataPipelineCommand.Search => ReturnOK(dataContext),

//            DataPipelineCommand.AppendList => ReturnOK(dataContext),
//            DataPipelineCommand.GetList => ReturnNotFound(dataContext),
//            DataPipelineCommand.DeleteList => ReturnOK(dataContext),
//            DataPipelineCommand.SearchList => ReturnOK(dataContext),

//            DataPipelineCommand.AcquireLock => ReturnNotFound(dataContext),
//            DataPipelineCommand.AcquireExclusiveLock => ReturnNotFound(dataContext),
//            DataPipelineCommand.ReleaseLock => ReturnOK(dataContext),

//            _ => throw new ArgumentOutOfRangeException($"Unknown command '{dataContext.Command}'"),
//        },

//        var handler => handler.Execute(dataContext, context),
//    };

//    private static Task<Option<DataPipelineContext>> ReturnOK(DataPipelineContext context) => new Option<DataPipelineContext>(context, StatusCode.OK).ToTaskResult();
//    private static Task<Option<DataPipelineContext>> ReturnNotFound(DataPipelineContext context) => new Option<DataPipelineContext>(StatusCode.NotFound).ToTaskResult();
//}