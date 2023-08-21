using System.Diagnostics;
using System.Reflection;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Block;

public interface IContractActor : IGrainWithStringKey
{
    Task<Option> Delete(string principalId, string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(ContractCreateModel blockCreateModel, string traceId);
    Task<Option<DataBlock>> GetLatest(string blockType, string principalId, string traceId);
    Task<Option<IReadOnlyList<DataBlock>>> List(string blockType, string principalId, string traceId);
    Task<Option> Append(DataBlock block, string traceId);
}


public class ContractActor : Grain, IContractActor
{
    private readonly IPersistentState<BlockChain> _state;
    private readonly ILogger<ContractActor> _logger;
    private readonly ISign _sign = null!;

    public ContractActor(
        [PersistentState(stateName: SpinConstants.Extension.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlockChain> state,
        ILogger<ContractActor> logger
        )
    {
        _state = state;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Contract, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = await new Option().ToTaskResult()
            .TestAsync(() => _state.RecordExists ? StatusCode.OK : StatusCode.BadRequest)
            .TestAsync(() => IdPatterns.IsPrincipalId(principalId) ? StatusCode.OK : StatusCode.BadRequest)
            .TestAsync(async () => await IsOwner(principalId, context).LogResult(context.Location()));
        if (test.IsError()) return test;

        context.Location().LogInformation("Deleted block chain, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    public async Task<Option> Create(ContractCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating block chain, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = new Option()
            .Test(() => !_state.RecordExists)
            .Test(() => model.Validate().LogResult(context.Location()));
        if (test.IsError()) return test;

        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetDocumentId(model.DocumentId)
            .SetPrincipleId(model.PrincipalId)
            .AddAccess(model.BlockAccess)
            .Build(_sign, context)
            .LogResult(context.Location());

        if (blockChain.IsError()) return blockChain.ToOptionStatus();

        _state.State = blockChain.Return();
        await _state.WriteStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option<DataBlock>> GetLatest(string blockType, string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting lastest, actorKey={actorKey}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), blockType, principalId);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => IdPatterns.IsBlockType(blockType))
            .Test(() => IdPatterns.IsPrincipalId(principalId));
        if (test.IsError()) return test.ToOptionStatus<DataBlock>();

        Option<BlockChain> readBlockChain = await ReadContract(context);
        if (readBlockChain.IsError()) return readBlockChain.ToOptionStatus<DataBlock>();

        BlockChain blockChain = readBlockChain.Return();
        Option<BlockReader<DataBlock>> stream = blockChain.GetReader(blockType, principalId);
        if (stream.IsError()) return stream.ToOptionStatus<DataBlock>();

        var latest = stream.Return().GetLatest();
        return latest;
    }

    public async Task<Option<IReadOnlyList<DataBlock>>> List(string blockType, string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting list, actorKey={actorKey}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), blockType, principalId);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => IdPatterns.IsBlockType(blockType))
            .Test(() => IdPatterns.IsPrincipalId(principalId));
        if (test.IsError()) return test.ToOptionStatus<IReadOnlyList<DataBlock>>();

        Option<BlockChain> readBlockChain = await ReadContract(context);
        if (readBlockChain.IsError()) return readBlockChain.ToOptionStatus<IReadOnlyList<DataBlock>>();

        BlockChain blockChain = readBlockChain.Return();
        Option<BlockReader<DataBlock>> stream = blockChain.GetReader(blockType, principalId);
        if (stream.IsError()) return stream.ToOptionStatus<IReadOnlyList<DataBlock>>();

        IReadOnlyList<DataBlock> list = stream.Return().List();
        return list.ToOption();
    }

    public async Task<Option> Append(DataBlock block, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Append, actorKey={actorKey}, blockId={blockId}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), block.BlockId, block.BlockType, block.PrincipleId);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => block.Validate());
        if (test.IsError()) return test;

        Option<BlockChain> readBlockChain = await ReadContract(context);
        if (readBlockChain.IsError()) return readBlockChain.ToOptionStatus();

        BlockChain blockChain = readBlockChain.Return();
        var addOption = blockChain.Add(block);
        return addOption;
    }

    private async Task<Option<BlockChain>> ReadContract(ScopeContext context)
    {
        context.Location().LogInformation("Reading BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ReadStateAsync();
        if (!_state.RecordExists) return new Option<BlockChain>(StatusCode.NotFound);

        var v = await ValidateBlock(_state.State, context);
        if (v.IsError()) return v.ToOptionStatus<BlockChain>();

        return _state.State;
    }

    private async Task<Option> WriteContract(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Writing SoftBank acocunt");

        var v = await ValidateBlock(blockChain, context);
        if (v.IsError()) return v;

        _state.State = blockChain;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    private async Task<Option> ValidateBlock(BlockChain blockChain, ScopeContext context)
    {
        return StatusCode.OK;
    }

    private async Task<Option> IsOwner(string principalId, ScopeContext context)
    {
        Option<BlockChain> blockChain = await ReadContract(context);
        if (blockChain.IsError()) return blockChain.ToOptionStatus();

        if (blockChain.Return().IsOwner(principalId).IsError()) return StatusCode.Forbidden;

        return StatusCode.OK;
    }
}
