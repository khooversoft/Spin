using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public interface IContractActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(ContractCreateModel blockCreateModel, string traceId);
    Task<Option<ContractQueryResponse>> Query(ContractQuery model, string traceId);
    Task<Option> Append(DataBlock block, string traceId);
    Task<Option<ContractPropertyModel>> GetProperties(string principalId, string traceId);
    Task<Option> HasAccess(string principalId, BlockRoleGrant grant, string traceId);
    Task<Option> HasAccess(string principalId, BlockGrant grant, string blockType, string traceId);
}

public class ContractActor : Grain, IContractActor
{
    private readonly IPersistentState<BlockChain> _state;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ContractActor> _logger;
    private bool _blockVerified = false;

    public ContractActor(
        [PersistentState(stateName: SpinConstants.Ext.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlockChain> state,
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

        if (!_state.RecordExists) return StatusCode.NotFound;

        context.Location().LogInformation("Deleted block chain, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public async Task<Option> Exist(string traceId)
    {
        await _state.ReadStateAsync();
        return _state.RecordExists ? StatusCode.OK : StatusCode.NotFound;
    }

    public async Task<Option> Create(ContractCreateModel model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating block chain, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (_state.RecordExists) return StatusCode.Conflict;
        if (!model.Validate(out var v)) return v;

        ISignatureActor signature = _clusterClient.GetResourceGrain<ISignatureActor>(SpinConstants.SignValidation);

        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetDocumentId(model.DocumentId)
            .SetPrincipleId(model.PrincipalId)
            .AddAccess(model.BlockAccess)
            .Build(signature, context);

        if (blockChain.IsError()) return blockChain.ToOptionStatus();

        await WriteContract(blockChain.Return(), context);
        return StatusCode.OK;
    }

    public async Task<Option<ContractQueryResponse>> Query(ContractQuery model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Query, actorKey={actorKey}, model={model}", this.GetPrimaryKeyString(), model);

        if (!_state.RecordExists) return StatusCode.NotFound;
        if (!model.Validate(out var mv)) return mv.ToOptionStatus<ContractQueryResponse>();

        await VerifyBlock();

        string? blockTypes = model.GetBlockTypes();
        Option<IEnumerable<DataBlock>> dataBlocks = blockTypes switch
        {
            string v => _state.State.Filter(model.PrincipalId, v),
            _ => _state.State.Filter(model.PrincipalId),
        };

        if (dataBlocks.IsError()) return dataBlocks.ToOptionStatus<ContractQueryResponse>();

        var response = new ContractQueryResponse
        {
            Items = dataBlocks.Return()
                .GroupBy(x => x.BlockType)
                .Select(x => new QueryBlockTypeResponse
                {
                    BlockType = x.Key,
                    DataBlocks = model.LatestOnly(x.Key) ? x.TakeLast(1).ToArray() : x.ToArray(),
                })
                .ToArray(),
        };

        return response.ToOption();
    }

    public async Task<Option> Append(DataBlock block, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Append, actorKey={actorKey}, blockId={blockId}, blockType={blockType}, principalId={principalId}",
            this.GetPrimaryKeyString(), block.BlockId, block.BlockType, block.PrincipleId);

        if (!_state.RecordExists) return StatusCode.NotFound;
        if (!block.Validate(out var mv)) return mv;
        if (!_state.State.HasAccess(block.PrincipleId, BlockGrant.Write, block.BlockType, out var av)) return av;

        await VerifyBlock();

        var addOption = _state.State.Add(block);
        if (addOption.IsError()) return addOption;

        var writeOption = await WriteContract(_state.State, context);
        return writeOption;
    }

    public async Task<Option<ContractPropertyModel>> GetProperties(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("GetProperties, actorKey={actorKey}, principalId={principalId}", this.GetPrimaryKeyString(), principalId);

        if (!_state.RecordExists) return new Option<ContractPropertyModel>(StatusCode.BadRequest, "Record does not exist");
        await VerifyBlock();
        if (!_state.State.HasAccess(principalId, BlockRoleGrant.Owner, out var v)) return v.ToOptionStatus<ContractPropertyModel>();

        GenesisBlock genesis = _state.State.GetGenesisBlock();
        Option<AclBlock> acl = _state.State.GetAclBlock(principalId);

        var response = new ContractPropertyModel
        {
            DocumentId = genesis.DocumentId,
            OwnerPrincipalId = genesis.OwnerPrincipalId,
            BlockAcl = acl.HasValue ? acl.Value.AccessRights.ToArray() : Array.Empty<AccessBlock>(),
            BlockCount = _state.State.Count,
        };

        return response.ToOption();
    }

    public async Task<Option> HasAccess(string principalId, BlockRoleGrant grant, string traceId)
    {
        if (!_state.RecordExists) return StatusCode.BadRequest;

        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("HasRoleAccess, actorKey={actorKey}, principalId={principalId}, grant={grant}",
            this.GetPrimaryKeyString(), principalId, grant);

        await VerifyBlock();

        var result = _state.State.HasAccess(principalId, grant);
        return result;

    }

    public async Task<Option> HasAccess(string principalId, BlockGrant grant, string blockType, string traceId)
    {
        if (!_state.RecordExists) return StatusCode.BadRequest;

        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("HasAccess, actorKey={actorKey}, principalId={principalId}, grant={grant}, blockType={blockType}",
            this.GetPrimaryKeyString(), principalId, grant, blockType);

        await VerifyBlock();

        var result = _state.State.HasAccess(principalId, grant, blockType);
        return result;
    }

    private async Task<Option> WriteContract(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Writing block chain");

        _blockVerified = false;

        var v = await ValidateBlock(blockChain, context);
        if (v.IsError()) return v;

        _blockVerified = true;

        _state.State = blockChain;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    private async Task VerifyBlock()
    {
        if (_state.RecordExists && !_blockVerified)
        {
            var context = new ScopeContext(_logger);
            (await ValidateBlock(_state.State, context)).Assert(x => x.IsOk(), _ => $"Cannot validate block chain, actorKey={this.GetPrimaryKeyString()}");

            _blockVerified = true;
        }
    }

    private async Task<Option> ValidateBlock(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Verifying signatures in block chain");

        ISignatureActor signature = _clusterClient.GetResourceGrain<ISignatureActor>(SpinConstants.SignValidation);
        var result = await blockChain.ValidateBlockChain(signature, context);

        return result;
    }
}
