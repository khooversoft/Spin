using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Models
{
    public record MessageHostOption
    {
        public IReadOnlyList<MessageNodeOption> Nodes { get; init; } = null!;
    }


    public static class MessageHostOptionExtensions
    {
        public static void Verify(this MessageHostOption subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Nodes
                .VerifyNotNull(nameof(subject.Nodes))
                .VerifyAssert(x => x.Count > 0, "Nodes are empty");

            subject.Nodes.ForEach(x => x.Verify());

            subject.Nodes
                .GroupBy(x => (string)x.ServiceId)
                .Where(x => x.Count() > 1)
                .VerifyAssert(x => !x.Any(), x => $"Duplicate endpoint(s): {string.Join(", ", x)}");
        }
    }
}