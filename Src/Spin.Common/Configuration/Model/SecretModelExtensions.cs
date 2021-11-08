//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Toolbox.Tools;

//namespace Spin.Common.Configuration.Model
//{
//    public static class SecretModelExtensions
//    {
//        public static IEnumerable<KeyValuePair<string, string>> GetData(this SecretRecord model)
//        {
//            model.VerifyNotNull(nameof(model));
//            return (IEnumerable<KeyValuePair<string, string>>)model.Data ?? Array.Empty<KeyValuePair<string, string>>();
//        }

//        public static string? Get(this SecretRecord model, string key)
//        {
//            model.VerifyNotNull(nameof(model));
//            key.VerifyNotEmpty(nameof(key));

//            return model.GetData()
//                .Where(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
//                .Select(x => x.Value)
//                .FirstOrDefault();
//        }

//        public static SecretRecord SetWith(this SecretRecord model, string key, string secret)
//        {
//            model.VerifyNotNull(nameof(model));
//            key.VerifyNotEmpty(nameof(key));
//            secret.VerifyNotEmpty(nameof(secret));

//            return model with
//            {
//                Data = model.GetData()
//                    .Where(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
//                    .Append(new KeyValuePair<string, string>(key, secret))
//                    .ToDictionary(x => x.Key, x => x.Value)
//            };
//        }

//        public static SecretRecord DeleteWith(this SecretRecord model, string key)
//        {
//            model.VerifyNotNull(nameof(model));
//            key.VerifyNotEmpty(nameof(key));

//            return model with
//            {
//                Data = model.GetData()
//                    .Where(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
//                    .ToDictionary(x => x.Key, x => x.Value)
//            };
//        }
//    }
//}
