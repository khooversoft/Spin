using Identity.sdk.Models;
using Identity.sdk.Types;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.sdk.Client
{
    public class SignatureClient : ClientBase<Signature>
    {
        public SignatureClient(HttpClient httpClient, ILogger logger)
            : base(httpClient, "signature", logger)
        {
        }

        public async Task<Signature?> Get(IdentityId signatureId, CancellationToken token = default) =>
            await Get(Signature.ToArtifactId(signatureId), token);

        public async Task<bool> Delete(IdentityId signatureId, CancellationToken token = default) =>
            await Delete(Signature.ToArtifactId(signatureId), token);
    }
}