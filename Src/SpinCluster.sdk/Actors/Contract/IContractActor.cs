using SpinCluster.abstraction;
using Toolbox.Block;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public interface IContractActor : IGrainWithStringKey
{
    Task<Option> Append(DataBlock block, string traceId);
    Task<Option> Create(ContractCreateModel blockCreateModel, string traceId);
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<ContractPropertyModel>> GetProperties(string principalId, string traceId);
    Task<Option> HasAccess(string principalId, BlockRoleGrant grant, string traceId);
    Task<Option> HasAccess(string principalId, BlockGrant grant, string blockType, string traceId);
    Task<Option<ContractQueryResponse>> Query(ContractQuery model, string traceId);
}
