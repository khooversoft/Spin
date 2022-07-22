using Toolbox.Abstractions;
using Toolbox.Actor;

namespace Smart_Installment.sdk.Actor
{
    public interface IInstallmentContractActor : IActor
    {
        Task Append(InstallmentContract contract, CancellationToken token);
        Task CreateContract(InstallmentHeader header, CancellationToken token);
        Task<InstallmentContract?> Get(CancellationToken token);
    }
}