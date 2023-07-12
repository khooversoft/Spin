using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Toolbox.Test.Security;

public class SerializationTests
{
    [Fact]
    public void VerifyRsaSerialization()
    {
        using RSA rsa = RSA.Create();

        RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: true);
        byte[] pubk = rsa.ExportRSAPublicKey();
        byte[] privk = rsa.ExportRSAPrivateKey();

        string xml = rsa.ToXmlString(includePrivateParameters: true);
        string xml1 = rsa.ToXmlString(includePrivateParameters: true);

        using RSA rsa1 = RSA.Create();
        rsa1.FromXmlString(xml);

        RSAParameters publicKey2 = rsa1.ExportParameters(includePrivateParameters: false);
        RSAParameters privateKey2 = rsa1.ExportParameters(includePrivateParameters: true);

        string xml21 = rsa1.ToXmlString(includePrivateParameters: true);
    }

}
