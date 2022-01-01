using Artifact.sdk.Model;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Artifact.sdk.Client
{
    public class ArtifactMessageClient
    {
        private readonly IMessageHost _messageHost;
        private readonly ILogger<ArtifactMessageClient> _logger;
        private static readonly MessageUrl _messageUrl = new MessageUrl("message://artifact");

        public ArtifactMessageClient(IMessageHost messageHost, ILogger<ArtifactMessageClient> logger)
        {
            _messageHost = messageHost;
            _logger = logger;
        }

        public async Task Set(ArtifactPayload articlePayload, CancellationToken token = default)
        {
            articlePayload.VerifyNotNull(nameof(articlePayload));

            _logger.LogTrace($"{nameof(Set)}: Id={articlePayload.Id}");

            try
            {
                bool status = await _messageHost.Client.Post(_messageUrl, articlePayload);
                status.VerifyAssert(x => x == true, $"Set failed to {_messageUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Set)}: Failed");
                throw;
            }
        }

        public async Task<bool> Delete(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Delete)}: Id={id}");

            try
            {
                return await _messageHost.Client.Delete(_messageUrl, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Delete)}: Failed");
                throw;
            }
        }
    }
}