using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Artifact.sdk.Model
{
    public class ArtifactPayloadBuilder
    {
        public ArtifactPayloadBuilder() { }

        public ArtifactPayloadBuilder(ArtifactId id, string payload)
        {
            SetId(id);
            SetPayload(payload);
        }

        public ArtifactPayloadBuilder(ArtifactId id, byte[] data)
        {
            SetId(id);
            SetPayload(data);
        }

        public ArtifactId? Id { get; set; }

        public string? PayloadType { get; set; }

        public byte[]? Payload { get; set; }

        public ArtifactPayloadBuilder SetId(ArtifactId id) => this.Action(x => x.Id = id);

        public ArtifactPayloadBuilder SetPayloadType(string payload) => this.Action(x => x.PayloadType = payload);

        public ArtifactPayloadBuilder SetPayload(string payload) => this.Action(x => SetPayload(typeof(string).Name, payload.ToBytes()));

        public ArtifactPayloadBuilder SetPayload<T>(T subject) => this.Action(x => SetPayload(typeof(T).Name, Json.Default.Serialize(subject).ToBytes()));

        public ArtifactPayloadBuilder SetPayload(string payloadType, byte[] payload)
        {
            Payload = payload;
            PayloadType = payloadType;
            return this;
        }

        public ArtifactPayload Build()
        {
            Id.VerifyNotNull("Id is required");
            PayloadType.VerifyNotEmpty("Payload type is required");
            Payload.VerifyAssert(x => x?.Length > 0, $"{nameof(Payload)} is empty");

            var payload = new ArtifactPayload
            {
                Id = (string)Id,
                PackagePayload = Convert.ToBase64String(Payload!),
                Hash = Convert.ToBase64String(MD5.Create().ComputeHash(Payload!)),
            };

            payload.Verify();
            return payload;
        }
    }
}
