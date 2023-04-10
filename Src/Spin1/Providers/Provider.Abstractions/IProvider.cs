using SpinNet.sdk.Model;

namespace Provider.Abstractions;

public interface IProvider
{
    Task<NetResponse> Post(NetMessage message, CancellationToken token);
}