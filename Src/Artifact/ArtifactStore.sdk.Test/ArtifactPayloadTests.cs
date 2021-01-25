using ArtifactStore.sdk.Model;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArtifactStore.sdk.Test
{
    public class ArtifactPayloadTests
    {
        [Fact]
        public void GivenPayloadSource_ShouldRoundTrip()
        {
            ArtifactId artifactId = new ArtifactId("namespace/file.ext");
            string payload = "This is the payload";

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            ArtifactPayload artifactPayload = payloadBytes.ToArtifactPayload(artifactId);

            byte[] fromPayload = artifactPayload.ToBytes();
            Enumerable.SequenceEqual(payloadBytes, fromPayload).Should().BeTrue();
        }
    }
}
