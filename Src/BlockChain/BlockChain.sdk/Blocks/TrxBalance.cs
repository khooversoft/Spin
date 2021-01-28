using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.BlockDocument;
using Toolbox.Types;

namespace BlockChain.sdk.Blocks
{
    public class TrxBalance : TrxBlock
    {
        public TrxBalance(string referenceId, MaskDecimal4 value, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : base(referenceId, Constants.Balance, value, properties)
        {
        }
    }
}
