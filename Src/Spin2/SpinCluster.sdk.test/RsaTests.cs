using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpinCluster.sdk.test;

public class RsaTests
{
    [Fact]
    public void TestSigning()
    {
        var rsa = RSA.Create();

        RSAParameters rSAParameters = rsa.ExportParameters(true);


    }
}
