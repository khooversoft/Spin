using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestingHost;
using SpinCluster.sdk.Actors.Signature;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public record User
{
    private ISignatureActor _signatureActor;

    public User(TestCluster cluster, string ownerId, string keyId)
    {
        OwnerId = ownerId;
        KeyId = keyId;

        _signatureActor = cluster.GrainFactory.GetGrain<ISignatureActor>(KeyId);
    }

    public string OwnerId { get; }
    public string KeyId { get; }

    public async Task Createkey(ScopeContext context)
    {
        var request = new PrincipalKeyRequest
        {
            KeyId = KeyId,
            OwnerId = OwnerId,
            Audience = "test.com",
            Name = "test sign key",
        };

        await Delete(context);

        SpinResponse result = await _signatureActor.Create(request, context.TraceId);
        result.StatusCode.IsOk().Should().BeTrue();
    }

    public async Task Delete(ScopeContext context) => await _signatureActor.Delete(context.TraceId);
}
