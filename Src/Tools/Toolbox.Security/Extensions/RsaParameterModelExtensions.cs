using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public static class RsaParameterModelExtensions
    {
        private static readonly IReadOnlyList<byte> _empty = Enumerable.Empty<byte>().ToList();

        public static RsaParameterModel ConvertTo(this RSAParameters rSAParameters)
        {
            return new RsaParameterModel
            {
                D = rSAParameters.D?.ToList() ?? _empty,
                DP = rSAParameters.DP?.ToList() ?? _empty,
                DQ = rSAParameters.DQ?.ToList() ?? _empty,
                Exponent = rSAParameters.Exponent?.ToList() ?? _empty,
                InverseQ = rSAParameters.InverseQ?.ToList() ?? _empty,
                Modulus = rSAParameters.Modulus?.ToList() ?? _empty,
                P = rSAParameters.P?.ToList() ?? _empty,
                Q = rSAParameters.Q?.ToList() ?? _empty,
            };
        }

        public static RSAParameters ConvertTo(this RsaParameterModel rSAParametersModel)
        {
            return new RSAParameters
            {
                D = rSAParametersModel.D?.ToArray(),
                DP = rSAParametersModel.DP?.ToArray(),
                DQ = rSAParametersModel.DQ?.ToArray(),
                Exponent = rSAParametersModel.Exponent?.ToArray(),
                InverseQ = rSAParametersModel.InverseQ?.ToArray(),
                Modulus = rSAParametersModel.Modulus?.ToArray(),
                P = rSAParametersModel.P?.ToArray(),
                Q = rSAParametersModel.Q?.ToArray(),
            };
        }

        public static string ToJson(this RsaParameterModel subject)
        {
            subject.VerifyNotNull(nameof(subject));

            return JsonConvert.SerializeObject(subject);
        }

        public static RsaParameterModel ToRasParameterModel(this string json)
        {
            json.VerifyNotEmpty(nameof(json));

            return JsonConvert.DeserializeObject<RsaParameterModel>(json);
        }

        public static string ToBinaryString(this RSAParameters subject)
        {
            subject.VerifyNotNull(nameof(subject));

            RsaParameterModel model = subject.ConvertTo();

            string json = Json.Default.Serialize(model);
            return Convert.ToBase64String(json.ToBytes());
        }

        public static RSAParameters ToRSAParameters(this string subject)
        {
            subject.VerifyNotEmpty(nameof(subject));

            byte[] array = Convert.FromBase64String(subject);
            RsaParameterModel model = array.ToObject<RsaParameterModel>().VerifyNotNull("Failed to serialize");

            return model.ConvertTo();
        }
    }
}