using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Data;
using Toolbox.Types;

namespace Toolbox.Test.Data.Graph.Command;

public class GraphPayloadTests
{

    [Fact]
    public void AddPayloadToNode()
    {
        GraphMap map = new GraphMap();

        var r1 = new Record1
        {
            Name = "name",
            Address = "address",
            DatePurchased = DateTime.UtcNow,
            Value = 100,
        };

        var t = new Tags()
            .Set("t1")
            .Set(r1);

        string query = $"add node key=node1,tags={t};";

    }

    private record Record1
    {
        public string Name { get; init; } = null!;
        public string? Address { get; init; }
        public DateTime DatePurchased { get; init; }
        public int Value { get; init; }
        public int? ValueOption { get; init; }
    }
}
