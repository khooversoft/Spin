//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MessageNet.sdk.Host.Model;
//using Spin.Common.Configuration.Model;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Spin.Common.Configuration.Model
//{
//    public static class EnviromentModelExtensions
//    {
//        public static void Verify(this EnvironmentModel model)
//        {
//            model.VerifyNotNull(nameof(model));

//            model.Storage.ForEach(x => x.Verify());
//            model.Queue.ForEach(x => x.Verify());
//        }

//        public static EnvironmentModel AddWith(this EnvironmentModel model, StorageRecord storageModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            storageModel.VerifyNotNull(nameof(storageModel));

//            return model with
//            {
//                Storage = model.Storage
//                    .Concat(new[] { storageModel })
//                    .Distinct(EqualityComparerFactory.Create<StorageRecord>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
//                    .ToList(),
//            };
//        }

//        public static EnvironmentModel RemoveWith(this EnvironmentModel model, StorageRecord queueModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            queueModel.VerifyNotNull(nameof(queueModel));

//            return model with
//            {
//                Storage = model.Storage
//                    .Where(x => x?.Channel?.Equals(queueModel?.Channel, StringComparison.OrdinalIgnoreCase) ?? false)
//                    .ToList(),
//            };
//        }

//        public static EnvironmentModel AddWith(this EnvironmentModel model, QueueRecord queueRecord)
//        {
//            model.VerifyNotNull(nameof(model));
//            queueRecord.VerifyNotNull(nameof(queueRecord));

//            return model with
//            {
//                Queue = model.Queue
//                    .Concat(new[] { queueRecord })
//                    .Distinct(EqualityComparerFactory.Create<QueueRecord>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
//                    .ToList(),
//            };
//        }

//        public static EnvironmentModel RemoveWith(this EnvironmentModel model, string channel)
//        {
//            model.VerifyNotNull(nameof(model));
//            channel.VerifyNotEmpty(nameof(channel));

//            return model with
//            {
//                Queue = model.Queue
//                    .Where(x => x?.Channel?.Equals(channel, StringComparison.OrdinalIgnoreCase) ?? false)
//                    .ToList(),
//            };
//        }
//    }
//}
