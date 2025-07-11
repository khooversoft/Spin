using Microsoft.Extensions.Hosting;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Data.Client.Common;

public static class DataClientSingleFileUpdate
{
    public record DataClientSingleFileUpdateLogging(string Key);

    public static async Task SingleThreadFileUpdate(IHost host, string pipelineName, int count)
    {
        const string key = nameof(SingleThreadFileUpdate);

        host.NotNull();
        IDataClient<EntityMaster> dataHandler = host.Services.GetDataClient<EntityMaster>(pipelineName);
        var context = host.Services.CreateContext<DataClientSingleFileUpdateLogging>();

        var entityMaster = new EntityMaster();

        context.LogWarning("SingleThreadFileUpdate: Writing first");
        await write(entityMaster);
        foreach (var index in Enumerable.Range(0, count))
        {
            entityMaster.Entities.Add(new EntityModel
            {
                Name = $"Name-{index}",
                Index = index
            });

            context.LogWarning("SingleThreadFileUpdate: Writing others");
            await write(entityMaster);
        }

        async Task write(EntityMaster subject)
        {
            context.LogWarning("SingleThreadFileUpdate: Set, count={count}", subject.Entities.Count);
            var result = await dataHandler.Set(key, subject, context).ConfigureAwait(false);
            result.BeOk();

            context.LogWarning("SingleThreadFileUpdate: Get");
            var readOption = await dataHandler.Get(key, context).ConfigureAwait(false);
            readOption.BeOk();
            context.LogWarning("SingleThreadFileUpdate: Get, count={count}", readOption.Return().Entities.Count);
            (readOption.Return() == subject).BeTrue($"Failed to match sourceCount={subject.Entities.Count}, readCount={readOption.Return().Entities.Count}");
        }
    }

    public sealed record class EntityMaster
    {
        public List<EntityModel> Entities { get; init; } = new List<EntityModel>();

        public bool Equals(EntityMaster? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Entities.SequenceEqual(other.Entities);
        }

        public override int GetHashCode() => HashCode.Combine(Entities);
    }

    public record EntityModel
    {
        public string Key { get; init; } = Guid.NewGuid().ToString();
        public DateTime Date { get; init; } = DateTime.UtcNow;
        public string Name { get; init; } = null!;
        public int Index { get; init; }
    }
}
