using System;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Spin.Common.Configuration
{
    public static class EnviromentConfigModelExtensions
    {
        public static void Verify(this EnviromentConfigModel model)
        {
            model.VerifyNotNull(nameof(model));

            (model.Storages ?? Array.Empty<StorageModel>()).ForEach(x => x.Verify());
            (model.Queue ?? Array.Empty<QueueModel>()).ForEach(x => x.Verify());
        }

        public static EnviromentConfigModel AddWith(this EnviromentConfigModel model, StorageModel storageModel)
        {
            model.VerifyNotNull(nameof(model));
            storageModel.VerifyNotNull(nameof(storageModel));

            return model with
            {
                Storages = (model.Storages ?? Array.Empty<StorageModel>())
                    .Concat(new[] { storageModel })
                    .Distinct(EqualityComparerFactory.Create<StorageModel>((y, x) => x == y))
                    .ToArray(),
            };
        }

        public static EnviromentConfigModel RemoveWith(this EnviromentConfigModel model, StorageModel queueModel)
        {
            model.VerifyNotNull(nameof(model));
            queueModel.VerifyNotNull(nameof(queueModel));

            return model with
            {
                Storages = (model.Storages ?? Array.Empty<StorageModel>())
                    .Where(x => x != queueModel)
                    .ToArray(),
            };
        }

        public static EnviromentConfigModel AddWith(this EnviromentConfigModel model, QueueModel queueModel)
        {
            model.VerifyNotNull(nameof(model));
            queueModel.VerifyNotNull(nameof(queueModel));

            return model with
            {
                Queue = (model.Queue ?? Array.Empty<QueueModel>())
                    .Concat(new[] { queueModel })
                    .Distinct(EqualityComparerFactory.Create<QueueModel>((y, x) => x == y))
                    .ToArray(),
            };
        }

        public static EnviromentConfigModel RemoveWith(this EnviromentConfigModel model, QueueModel queueModel)
        {
            model.VerifyNotNull(nameof(model));
            queueModel.VerifyNotNull(nameof(queueModel));

            return model with
            {
                Queue = (model.Queue ?? Array.Empty<QueueModel>())
                    .Where(x => x != queueModel)
                    .ToArray(),
            };
        }
    }
}
