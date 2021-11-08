using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public static class ContentExtensions
    {
        public static void Verify(this Content subject)
        {
            subject.VerifyNotNull(nameof(subject));
            subject.ContentType.VerifyNotEmpty(nameof(subject.ContentType));
            subject.Data.VerifyNotEmpty(nameof(subject.Data));
        }

        public static Content ToContent<T>(this T subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return new Content
            {
                ContentType = subject.GetType().ToString(),
                Data = Json.Default.Serialize(subject),
            };
        }

        public static T ConvertTo<T>(this Content subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return Json.Default.Deserialize<T>(subject.Data).VerifyNotNull("Deserialize failed");
        }
    }
}
