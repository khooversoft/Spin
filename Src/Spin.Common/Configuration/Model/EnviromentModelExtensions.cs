using System;
using System.Collections.Generic;
using System.Linq;
using Spin.Common.Configuration.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Spin.Common.Configuration.Model
{
    public static class EnviromentModelExtensions
    {
        public static IEnumerable<StorageModel> GetStorage(this EnvironmentModel model) => model.Storage ?? Array.Empty<StorageModel>();

        public static IEnumerable<QueueModel> GetQueue(this EnvironmentModel model) => model.Queue?? Array.Empty<QueueModel>();

        public static void Verify(this EnvironmentModel model)
        {
            model.VerifyNotNull(nameof(model));

            model.GetStorage().ForEach(x => x.Verify());
            model.GetQueue().ForEach(x => x.Verify());
        }

        public static EnvironmentModel AddWith(this EnvironmentModel model, StorageModel storageModel)
        {
            model.VerifyNotNull(nameof(model));
            storageModel.VerifyNotNull(nameof(storageModel));

            return model with
            {
                Storage = model.GetStorage()
                    .Concat(new[] { storageModel })
                    .Distinct(EqualityComparerFactory.Create<StorageModel>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToArray(),
            };
        }

        public static EnvironmentModel RemoveWith(this EnvironmentModel model, StorageModel queueModel)
        {
            model.VerifyNotNull(nameof(model));
            queueModel.VerifyNotNull(nameof(queueModel));

            return model with
            {
                Storage = model.GetStorage()
                    .Where(x => x?.Channel?.Equals(queueModel?.Channel, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToArray(),
            };
        }

        public static EnvironmentModel AddWith(this EnvironmentModel model, QueueModel queueModel)
        {
            model.VerifyNotNull(nameof(model));
            queueModel.VerifyNotNull(nameof(queueModel));

            return model with
            {
                Queue = model.GetQueue()
                    .Concat(new[] { queueModel })
                    .Distinct(EqualityComparerFactory.Create<QueueModel>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToArray(),
            };
        }

        public static EnvironmentModel RemoveWith(this EnvironmentModel model, string channel)
        {
            model.VerifyNotNull(nameof(model));
            channel.VerifyNotEmpty(nameof(channel));

            return model with
            {
                Queue = model.GetQueue()
                    .Where(x => x?.Channel?.Equals(channel, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToArray(),
            };
        }
    }
}
