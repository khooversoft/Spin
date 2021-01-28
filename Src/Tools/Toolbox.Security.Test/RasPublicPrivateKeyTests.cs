using FluentAssertions;
using System;
using System.Linq;
using System.Security.Cryptography;
using Xunit;

namespace Toolbox.Security.Test
{
    public class RasPublicPrivateKeyTests
    {
        [Fact]
        public void GivenRasKey_WhenCreated_CanClone()
        {
            var key = new RsaPublicPrivateKey();

            var subject = new RsaPublicPrivateKey(key.Kid, key.PublicKey, (RSAParameters)key.PrivateKey);

            key.Kid.Should().Be(subject.Kid);

            using var rsa = RSA.Create((RSAParameters)key.PrivateKey);

            var r = rsa.ExportParameters(includePrivateParameters: true);

            RsaParameterModel rsaParameterModel = r.ConvertTo();
            string data1 = rsaParameterModel.ToJson();

            RSAParameters rSAParameters2 = data1.ToRasParameterModel().ConvertTo();

            r.D.SequenceEqual(rSAParameters2.D).Should().BeTrue();
            r.DP.SequenceEqual(rSAParameters2.DP).Should().BeTrue();
            r.DQ.SequenceEqual(rSAParameters2.DQ).Should().BeTrue();
            r.Exponent.SequenceEqual(rSAParameters2.Exponent).Should().BeTrue();
            r.InverseQ.SequenceEqual(rSAParameters2.InverseQ).Should().BeTrue();
            r.Modulus.SequenceEqual(rSAParameters2.Modulus).Should().BeTrue();
            r.P.SequenceEqual(rSAParameters2.P).Should().BeTrue();
            r.Q.SequenceEqual(rSAParameters2.Q).Should().BeTrue();
        }

        [Fact]
        public void GivenRasKey_WhenBinarySerialization_CanClone()
        {
            var key = new RsaPublicPrivateKey();

            var subject = new RsaPublicPrivateKey(key.Kid, key.PublicKey, (RSAParameters)key.PrivateKey);

            key.Kid.Should().Be(subject.Kid);

            using var rsa = RSA.Create((RSAParameters)key.PrivateKey);

            var r = rsa.ExportParameters(includePrivateParameters: true);

            string data1 = r.ToBinaryString();

            RSAParameters rSAParameters2 = data1.ToRSAParameters();

            r.D.SequenceEqual(rSAParameters2.D).Should().BeTrue();
            r.DP.SequenceEqual(rSAParameters2.DP).Should().BeTrue();
            r.DQ.SequenceEqual(rSAParameters2.DQ).Should().BeTrue();
            r.Exponent.SequenceEqual(rSAParameters2.Exponent).Should().BeTrue();
            r.InverseQ.SequenceEqual(rSAParameters2.InverseQ).Should().BeTrue();
            r.Modulus.SequenceEqual(rSAParameters2.Modulus).Should().BeTrue();
            r.P.SequenceEqual(rSAParameters2.P).Should().BeTrue();
            r.Q.SequenceEqual(rSAParameters2.Q).Should().BeTrue();
        }
    }
}