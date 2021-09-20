//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Spin.Common.Configuration.Model;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Spin.Common.Configuration
//{
//    public static class EnviromentConfigModelExtensions
//    {
//        public static IEnumerable<StorageModel> GetStorage(this EnviromentConfigModel model) => model.Storage ?? Array.Empty<StorageModel>();

//        public static IEnumerable<QueueModel> GetQueue(this EnviromentConfigModel model) => model.Queue?? Array.Empty<QueueModel>();

//        public static void Verify(this EnviromentConfigModel model)
//        {
//            model.VerifyNotNull(nameof(model));

//            model.GetStorage().ForEach(x => x.Verify());
//            model.GetQueue().ForEach(x => x.Verify());
//        }

//        public static EnviromentConfigModel AddWith(this EnviromentConfigModel model, StorageModel storageModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            storageModel.VerifyNotNull(nameof(storageModel));

//            return model with
//            {
//                Storage = model.GetStorage()
//                    .Concat(new[] { storageModel })
//                    .Distinct(EqualityComparerFactory.Create<StorageModel>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
//                    .ToArray(),
//            };
//        }

//        public static EnviromentConfigModel RemoveWith(this EnviromentConfigModel model, StorageModel queueModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            queueModel.VerifyNotNull(nameof(queueModel));

//            return model with
//            {
//                Storage = model.GetStorage()
//                    .Where(x => x?.Channel?.Equals(queueModel?.Channel, StringComparison.OrdinalIgnoreCase) ?? false)
//                    .ToArray(),
//            };
//        }

//        public static EnviromentConfigModel AddWith(this EnviromentConfigModel model, QueueModel queueModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            queueModel.VerifyNotNull(nameof(queueModel));

//            return model with
//            {
//                Queue = model.GetQueue()
//                    .Concat(new[] { queueModel })
//                    .Distinct(EqualityComparerFactory.Create<QueueModel>((y, x) => x?.Channel?.Equals(y?.Channel, StringComparison.OrdinalIgnoreCase) ?? false))
//                    .ToArray(),
//            };
//        }

//        public static EnviromentConfigModel RemoveWith(this EnviromentConfigModel model, QueueModel queueModel)
//        {
//            model.VerifyNotNull(nameof(model));
//            queueModel.VerifyNotNull(nameof(queueModel));

//            return model with
//            {
//                Queue = model.GetQueue()
//                    .Where(x => x?.Channel?.Equals(queueModel?.Channel, StringComparison.OrdinalIgnoreCase) ?? false)
//                    .ToArray(),
//            };
//        }
//    }
//}
