using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.DocumentStore.Test;

public class JsonDocumentConstructionTests
{
    [Fact]
    public void JsonConstruction_ShouldPass()
    {
        var document = new Document
        {
            DocumentId = (DocumentId)"test/doc",
            ObjectClass = "this",
        };

        byte[] hash = DocumentTools.ComputeHash(document.DocumentId, document.ObjectClass, "data");
        document = document with { Hash = hash };

        JsonNode documentNode = JsonObject.Parse(document.ToJson()).NotNull();

        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        documentNode["Data"] = JsonObject.Parse(payload.ToJson());

        string json = documentNode.ToJsonString();

        var final = json.ToObject<FinalDocument>();
        final.Should().NotBeNull();
        (final!.Data == payload).Should().BeTrue();
    }

    private record Document
    {
        public DocumentId DocumentId { get; init; } = null!;
        public string ObjectClass { get; init; } = null!;
        public byte[] Hash { get; init; } = null!;
        public string? PrincipleId { get; init; }
    }

    private record Payload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }

    private record FinalDocument
    {
        public DocumentId DocumentId { get; init; } = null!;
        public string ObjectClass { get; init; } = null!;
        public byte[] Hash { get; init; } = null!;
        public string? PrincipleId { get; init; }
        public Payload? Data { get; init; }
    }
}
