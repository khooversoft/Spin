using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Document;
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

    public async Task<string?> Sign(SignRequest signRequest, CancellationToken token)
    {
        signRequest.Verify();

        _logger.LogTrace($"Sign for directoryId={signRequest.DirectoryId}, classObject={signRequest.ClassType}");

        IdentityEntry? identityEntry = signRequest.ClassType switch
        {
            ClassTypeName.User => await GetFromUser(signRequest.DirectoryId, token),
            ClassTypeName.Identity => await _identityService.Get((DocumentId)signRequest.DirectoryId, token),

            _ => throw new ArgumentException($"Unknown class type={signRequest.ClassType}"),
        };

        if (identityEntry == null)
        {
            _logger.LogError($"Cannot find signing data for directoryId={signRequest.DirectoryId}, classObject={signRequest.ClassType}");
            return null;
        }

        _logger.LogTrace($"Signing for directoryId={signRequest.DirectoryId}, classObject={signRequest.ClassType}");
        IPrincipalSignature principleSignature = new PrincipalSignature(signRequest.DirectoryId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

        string jwt = new JwtTokenBuilder()
            .SetDigest(signRequest.Digest)
            .SetPrincipleSignature(principleSignature)
            .SetExpires(DateTime.Now.AddYears(10))
            .SetIssuedAt(DateTime.Now)
            .Build();

        _logger.LogInformation($"Signed for directoryId={signRequest.DirectoryId}, classObject={signRequest.ClassType}");
        return jwt;
    }

    public async Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token)
    {
        _logger.LogTrace($"Validate for directoryId={validateRequest.DirectoryId}");

        IdentityEntry? identityEntry = validateRequest.ClassType switch
        {
            ClassTypeName.User => await GetFromUser(validateRequest.DirectoryId, token),
            ClassTypeName.Identity => await _identityService.Get((DocumentId)validateRequest.DirectoryId, token),

            _ => throw new ArgumentException($"Unknown class type={validateRequest.ClassType}"),
        };

        if (identityEntry == null)
        {
            _logger.LogError($"Cannot find signing data for directoryId={validateRequest.DirectoryId}");
            return false;
        }

        IPrincipalSignature principleSignature = new PrincipalSignature(validateRequest.DirectoryId, _issuer, _audience, identityEntry.Subject, identityEntry.GetRsaParameters());

        try
        {
            JwtTokenDetails tokenDetails = new JwtTokenParserBuilder()
                .SetPrincipleSignature(principleSignature)
                .Build()
                .Parse(validateRequest.Jwt);

            _logger.LogTrace($"JWT validated for directoryId={validateRequest.DirectoryId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed validation for directoryId={validateRequest.DirectoryId}");
            return false;
        }
    }

    private async Task<IdentityEntry?> GetFromUser(string directoryId, CancellationToken token)
    {
        _logger.LogTrace($"Getting signing directory id from user {directoryId}");
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
