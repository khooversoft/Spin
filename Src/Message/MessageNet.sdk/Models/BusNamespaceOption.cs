//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Models
//{
//    public record BusNamespaceOption
//    {
//        public string Namespace { get; init; } = null!;

//        public string BusNamespace { get; init; } = null!;

//        public string KeyName { get; init; } = null!;

//        public string AccessKey { get; init; } = null!;
//    }


//    public static class BusNamespaceoption
//    {
//        public static void Verify(this BusNamespaceOption subject)
//        {
//            subject.VerifyNotNull(nameof(subject));

//            subject.Namespace.VerifyNotEmpty(nameof(subject.Namespace));
//            subject.BusNamespace.VerifyNotEmpty(nameof(subject.BusNamespace));
//            subject.KeyName.VerifyNotEmpty(nameof(subject.KeyName));
//            subject.AccessKey.VerifyNotEmpty(nameof(subject.AccessKey));
//        }
//    }
//}
