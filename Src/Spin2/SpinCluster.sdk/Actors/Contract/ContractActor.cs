﻿using System.Diagnostics;
using System.Reflection;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;
using SpinCluster.sdk.Actors.Signature;

namespace SpinCluster.sdk.Actors.Contract;

public interface IContractActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(ContractCreateModel blockCreateModel, string traceId);
    Task<Option<IReadOnlyList<DataBlock>>> Query(ContractQuery model, string traceId);
    Task<Option> Append(DataBlock block, string traceId);
}

public class ContractActor : Grain, IContractActor
{
    private readonly IPersistentState<BlockChain> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ContractActor> _logger;

    public ContractActor(
        [PersistentState(stateName: SpinConstants.Extension.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlockChain> state,
        IClusterClient clusterClient,
        ILogger<ContractActor> logger
        )
    {
        _state = state.NotNull();
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.Contract, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());
        if (!_state.RecordExists) return StatusCode.BadRequest;

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

        ISignatureActor signature = _clusterClient.GetSignatureActor();

        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetDocumentId(model.DocumentId)
            .SetPrincipleId(model.PrincipalId)
            .AddAccess(model.BlockAccess)
            .Build(signature, context)
            .LogResult(context.Location());

        if (blockChain.IsError()) return blockChain.ToOptionStatus();

        await WriteContract(blockChain.Return(), context);

        return StatusCode.OK;
    }

    public async Task<Option<IReadOnlyList<DataBlock>>> Query(ContractQuery model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Query, actorKey={actorKey}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), model.BlockType, model.PrincipalId);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => model.Validate());
        if (test.IsError()) return test.ToOptionStatus<IReadOnlyList<DataBlock>>();

        Option<BlockChain> readBlockChain = await ReadContract(context);
        if (readBlockChain.IsError()) return readBlockChain.ToOptionStatus<IReadOnlyList<DataBlock>>();

        BlockChain blockChain = readBlockChain.Return();

        Option<IEnumerable<DataBlock>> stream = model.BlockType switch
        {
            string v => blockChain.Filter(model.PrincipalId, v),
            _ => blockChain.Filter(model.PrincipalId),
        };
        if (stream.IsError()) return stream.ToOptionStatus<IReadOnlyList<DataBlock>>();

        IReadOnlyList<DataBlock> result = model.LatestOnly switch
        {
            true => stream.Return().TakeLast(1).ToArray(),
            false => stream.Return().ToArray()
        };

        return result.ToOption();
    }

    public async Task<Option> Append(DataBlock block, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Append, actorKey={actorKey}, blockId={blockId}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), block.BlockId, block.BlockType, block.PrincipleId);

        var test = new Option()
            .Test(() => _state.RecordExists)
            .Test(() => block.Validate())
            .Test(() => _state.State.IsAuthorized(BlockGrant.Write, block.BlockType, block.PrincipleId));
        if (test.IsError()) return test;

        Option<BlockChain> readBlockChain = await ReadContract(context);
        if (readBlockChain.IsError()) return readBlockChain.ToOptionStatus();

        BlockChain blockChain = readBlockChain.Return();
        var addOption = blockChain.Add(block);
        if( addOption.IsError() ) return addOption;

        var writeOption = await WriteContract(blockChain, context);
        return writeOption;
    }

    private async Task<Option<BlockChain>> ReadContract(ScopeContext context)
    {
        context.Location().LogInformation("Reading block chain, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ReadStateAsync();
        if (!_state.RecordExists) return new Option<BlockChain>(StatusCode.NotFound);

        var v = await ValidateBlock(_state.State, context);
        if (v.IsError()) return v.ToOptionStatus<BlockChain>();

        return _state.State;
    }

    private async Task<Option> WriteContract(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Writing block chain");

        var v = await ValidateBlock(blockChain, context);
        if (v.IsError()) return v;

        _state.State = blockChain;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    private async Task<Option> ValidateBlock(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Verifying signatures in block chain");

        ISignatureActor signature = _clusterClient.GetSignatureActor();
        var result = await blockChain.ValidateBlockChain(signature, context).LogResult(context.Location());

        return result;
    }

    //private async Task<Option> IsOwner(string principalId, ScopeContext context)
    //{
    //    Option<BlockChain> blockChain = await ReadContract(context);
    //    if (blockChain.IsError()) return blockChain.ToOptionStatus();

    //    if (blockChain.Return().IsOwner(principalId).IsError()) return StatusCode.Forbidden;

    //    return StatusCode.OK;
    //}
}
