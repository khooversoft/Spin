using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using Spin.Common.Sign;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Protocol;
using Toolbox.Block;
using Toolbox.Block.Application;
using Toolbox.DocumentStore;
using Toolbox.Security;
using Toolbox.Security.Sign;

namespace Directory.sdk.Service;

public class SigningService
{
    private const string _issuer = "identity.com";
    private const string _audience = "spin.com";

    private readonly DirectoryService _directoryService;
    private readonly IdentityService _identityService;
    private readonly ILogger<SigningService> _logger;

    public SigningService(DirectoryService directoryService, IdentityService identityService, ILogger<SigningService> logger)
    {
        _directoryService = directoryService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<SignRequestResponse> Sign(SignRequest signRequest, CancellationToken token)
    {
        signRequest.Verify();

        _logger.LogTrace($"Sign for id={signRequest.Id}");
        List<PrincipleDigest> response = new List<PrincipleDigest>();
        List<string> errors = new();

        foreach (var request in signRequest.PrincipleDigests)
        {
            IdentityEntry? identityEntry = await GetIdentity(request.PrincipleId, token);

            if (identityEntry == null)
            {
                string msg = $"Cannot find signing data for directoryId={request.PrincipleId}";
                _logger.LogError(msg);
                errors.Add(msg);

                response.Add(request);
                continue;
            }

            _logger.LogTrace($"Signing for PrincipleId={request.PrincipleId}");
            IPrincipalSignature principleSignature = new PrincipalSignature(request.PrincipleId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

            string jwt = new JwtTokenBuilder()
                .SetDigest(request.Digest)
                .SetPrincipleSignature(principleSignature)
                .SetExpires(DateTime.Now.AddYears(10))
                .SetIssuedAt(DateTime.Now)
                .Build();

            _logger.LogInformation($"Signed for directoryId={request.PrincipleId}");

            response.Add(request with { JwtSignature = jwt });
        }

        return new SignRequestResponse
        {
            PrincipleDigests = response,
            Errors = errors,
        };
    }

    public async Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token)
    {
        _logger.LogTrace($"Validate for id={validateRequest.Id}");

        foreach (var request in validateRequest.PrincipleDigests)
        {
            IdentityEntry? identityEntry = await GetIdentity(request.PrincipleId, token);

            if (identityEntry == null || request.JwtSignature.IsEmpty())
            {
                _logger.LogError($"Cannot find signing data for PrincipleId={request.PrincipleId}");
                return false;
            }

            IPrincipalSignature principleSignature = new PrincipalSignature(request.PrincipleId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

            try
            {
                JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                    .SetPrincipleSignature(principleSignature)
                    .Build()
                    .Parse(request.JwtSignature);

                _logger.LogTrace($"JWT validated for PrincipleId={request.PrincipleId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed validation for PrincipleId={request.PrincipleId}");
                return false;
            }
        }

        return true;
    }

    private async Task<IdentityEntry?> GetIdentity(string directoryId, CancellationToken token)
    {
        _logger.LogTrace($"Getting signing directory id from user {directoryId}");
        DocumentId documentId = (DocumentId)directoryId;

        switch (documentId.Container)
        {
            case "identity":
                return await _identityService.Get(documentId, token, includePrivateKey: true);

            default:
                DirectoryEntry? directoryEntry = await _directoryService.Get((DocumentId)directoryId, token);
                if (directoryEntry == null)
                {
                    _logger.LogWarning($"Cannot find user ENTRY {directoryId} in directory");
                    return null;
                }

                string? identityId = directoryEntry.GetSigningCredentials();
                if (identityId == null)
                {
                    _logger.LogWarning($"Cannot find user {directoryId} signing credential property");
                    return null;
                }

                return await _identityService.Get((DocumentId)identityId, token, includePrivateKey: true);
        }
    }
}
