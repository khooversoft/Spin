using System.Collections.Generic;

namespace Toolbox.BlockDocument
{
    public interface IDataBlock
    {
        string GetDigest();

        public string Digest { get; }

        public string? JwtSignature { get; }

        void Validate();
    }
}