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

    public User(TestCluster cluster, PrincipalId ownerId, ObjectId keyId)
    {
        OwnerId = ownerId;
        KeyId = keyId;

        _signatureActor = cluster.GrainFactory.GetGrain<ISignatureActor>(KeyId);
    }

    public PrincipalId OwnerId { get; }
    public ObjectId KeyId { get; }

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

        Option result = await _signatureActor.Create(request, context.TraceId);
        result.StatusCode.IsOk().Should().BeTrue();
    }

    public async Task Delete(ScopeContext context) => await _signatureActor.Delete(context.TraceId);
}
