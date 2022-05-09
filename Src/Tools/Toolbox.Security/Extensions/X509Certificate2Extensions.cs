using System;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Tools;

namespace Toolbox.Security
{
    public static class X509Certificate2Extensions
    {
        public static bool IsExpired(this X509Certificate2 self)
        {
            self.NotNull(nameof(self));

            return DateTime.Now > self.NotAfter;
        }
    }
}