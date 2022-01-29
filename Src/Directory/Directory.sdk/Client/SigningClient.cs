using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Tools;

namespace Directory.sdk.Client
{
    public class SigningClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SigningClient> _logger;

        public SigningClient(HttpClient httpClient, ILogger<SigningClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> Sign(SignRequest signRequest, CancellationToken token = default)
        {
            _logger.LogTrace($"Signing request for directoryId={signRequest.DirectoryId}");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/signing/sign", signRequest, token);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token = default)
        {
            _logger.LogTrace($"Signing request for directoryId={validateRequest.DirectoryId}");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/signing/validate", validateRequest, token);
            response.EnsureSuccessStatusCode();

            return true;
        }
    }


    public static class SigningClientExtensions
    {
        public static BlockChainBuilder SetSign(this BlockChainBuilder subject, string principalId, SigningClient signingClient, CancellationToken token)
        {
            subject.VerifyNotNull(nameof(subject));
            principalId.VerifyNotEmpty(nameof(principalId));
            signingClient.VerifyNotNull(nameof(signingClient));
            
            subject.SetSign(async x =>
            {
                var signRequest = new SignRequest
                {
                    DirectoryId = principalId,
                    ClassType = ClassTypeName.User,
                    Digest = x
                };

                return await signingClient.Sign(signRequest, token);
            });

            return subject;
        }

        public static async Task Add<T>(this BlockChain subject, T value, string principalId, SigningClient signingClient, CancellationToken token)
        {
            subject.VerifyNotNull(nameof(subject));
            value.VerifyNotNull(nameof(value));
            principalId.VerifyNotEmpty(nameof(principalId));
            signingClient.VerifyNotNull(nameof(signingClient));

            await subject.Add(value, async x =>
            {
                var signRequest = new SignRequest
                {
                    DirectoryId = principalId,
                    ClassType = ClassTypeName.User,
                    Digest = x
                };

                return await signingClient.Sign(signRequest, token);
            });
        }
    }
}
