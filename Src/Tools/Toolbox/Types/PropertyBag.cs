using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types;

public class PropertyBag : ConcurrentDictionary<string, string>
{
    public PropertyBag() { }
    public PropertyBag(IEnumerable<KeyValuePair<string, string>> collection) : base(collection) { }
    public PropertyBag(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity) { }
}
