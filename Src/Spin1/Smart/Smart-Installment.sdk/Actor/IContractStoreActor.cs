using Toolbox.Abstractions;
using Toolbox.Actor;

namespace Smart_Installment.sdk.Actor
{
    public interface IContractStoreActor : IActor
    {
        Task Append(InstallmentContract contract, CancellationToken token);
        Task Create(InstallmentHeader header, CancellationToken token);
        Task<InstallmentContract?> Get(CancellationToken token);
    }
}