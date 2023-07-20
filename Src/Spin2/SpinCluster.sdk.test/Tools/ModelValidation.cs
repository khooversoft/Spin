using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Types;

namespace SpinCluster.sdk.test.Tools;

public class ModelValidation
{
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public void TestPrincipalKeyRequest()
    {
        string ownerId = "signKey@test.com";
        string keyId = $"principalKey/test/SignAndVerify/{ownerId}";

        var request = new PrincipalKeyRequest
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "audience",
            Name = "test sign key",
        };

        var validationResult = PrincipalKeyRequestValidator.Validator.Validate(request);
        validationResult.IsValid.Should().BeTrue();
    }
}
