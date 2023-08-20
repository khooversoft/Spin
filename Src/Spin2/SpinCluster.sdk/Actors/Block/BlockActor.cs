using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Block;

public interface IBlockActor : IGrainWithStringKey
{
    Task<Option> Delete(string principalId, string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(BlockCreateModel blockCreateModel, string traceId);
    Task<Option<DataBlock>> GetLatest(string blockType, string principalId, string traceId);
    Task<Option<IReadOnlyList<DataBlock>>> List<DataBlock>(string blockType, string principalId);
    Task<Option> Append(DataBlock block, string traceId);
}


public class BlockActor : Grain/*, IBlockActor*/
{
    private readonly IPersistentState<BlobPackage> _state;
    private readonly IValidator<BlobPackage> _blobPackageValidator;
    private readonly IValidator<BlockCreateModel> _blobCreateModelValidator;
    private readonly ILogger<BlockActor> _logger;

    public BlockActor(
        [PersistentState(stateName: SpinConstants.Extension.BlockStorage, storageName: SpinConstants.SpinStateStore)] IPersistentState<BlobPackage> state,
        IValidator<BlobPackage> blobPackageValidator,
        IValidator<BlockCreateModel> blobCreateModelValidator,
        ILogger<BlockActor> logger
        )
    {
        _state = state;
        _blobPackageValidator = blobPackageValidator;
        _blobCreateModelValidator = blobCreateModelValidator;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.VerifySchema(SpinConstants.Schema.BlockStorage, new ScopeContext(_logger));
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Option> Delete(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());

        if (!_state.RecordExists) return StatusCode.NotFound;

        var id = PrincipalId.Create(principalId).LogResult(context.Location());
        if (id.IsError()) return id.ToOptionStatus();

        Option<BlockChain> blockChain = await ReadContract(context).LogResult(context.Location());
        if (blockChain.IsError()) return blockChain.ToOptionStatus();

        if (!blockChain.Return().IsOwner(id.Return()).StatusCode.IsError()) return StatusCode.Forbidden;

        context.Location().LogInformation("Deleted BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());
        await _state.ClearStateAsync();

        return StatusCode.OK;
    }

    public Task<Option> Exist(string _) => new Option(_state.RecordExists ? StatusCode.OK : StatusCode.NotFound).ToTaskResult();

    //public async Task<Option> Create(BlockCreateModel model, string traceId)
    //{
    //    var context = new ScopeContext(traceId, _logger);
    //    context.Location().LogInformation("Deleting BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());

    //    if (!_state.RecordExists) return StatusCode.NotFound;

    //    var modelValidated = _blobCreateModelValidator.Validate(model).LogResult(context.Location());
    //    if (!modelValidated.IsValid) return StatusCode.BadRequest;

    //    Option<BlockChain> blockChain = await new BlockChainBuilder()
    //        .SetObjectId(model.ObjectId)
    //        .SetPrincipleId(model.PrincipalId)
    //        .AddAccess(model.BlockAccess)
    //        .Build(_sign, context)
    //        .LogResult(context.Location());

    //    if (blockChain.IsError()) return blockChain.ToOptionStatus<SoftBankAccount>();


    //}

    public Task<Option<BlobPackage>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Get BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var option = _state.RecordExists switch
        {
            true => _state.State.ToOption(),
            false => new Option<BlobPackage>(StatusCode.NotFound),
        };

        return option.ToTaskResult();
    }

    public async Task<Option> Set(BlobPackage model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set BlobPackage, actorKey={actorKey}", this.GetPrimaryKeyString());

        var test = new Option()
            .Test(() => _blobPackageValidator.Validate(model).LogResult(context.Location()).ToOptionStatus())
            .Test(() => this.VerifyIdentity(model.ObjectId));
        if (test.IsError()) return test;

        _state.State = model;
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

    private async Task<Option<BlockChain>> ReadContract(ScopeContext context)
    {
        context.Location().LogInformation("Reading BLOB for SoftBankAccount, actorKey={actorKey}", this.GetPrimaryKeyString());

        await _state.ReadStateAsync();
        if (!_state.RecordExists) return new Option<BlockChain>(StatusCode.NotFound);

        BlockChain blockChain = _state.State.ToObject<BlockChainModel>().ToBlockChain();

        //Option signResult = await contract.Return().ValidateBlockChain(context);
        //if (signResult.StatusCode.IsError())
        //{
        //    context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
        //    throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        //}

        return blockChain;
    }

    private async Task<Option> WriteContract(BlockChain blockChain, ScopeContext context)
    {
        context.Location().LogInformation("Writing SoftBank acocunt");

        //Option signResult = await contract.ValidateBlockChain(context);
        //if (signResult.StatusCode.IsError())
        //{
        //    context.Location().LogCritical("Contract actorId={actorId} could not be validated before writing to storage", this.GetPrimaryKeyString());
        //    throw new InvalidOperationException($"Contract should not be validated, actorId={this.GetPrimaryKeyString()}");
        //}

        _state.State = blockChain.ToBlobPackage();
        await _state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }
}
