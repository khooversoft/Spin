using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record DbNameId
{
    public DbNameId(string dbName) => (DbName, Values) = DbNameId.Parse(dbName).ThrowOnError().Return();
    public DbNameId(string dbName, IEnumerable<string> keys) => (DbName, Values) = (dbName.NotEmpty(), keys.NotNull().ToArray());
    public void Deconstruct(out string dbName, out IReadOnlyList<string> keys) => (dbName, keys) = (DbName, Values);

    public string DbName { get; }
    public IReadOnlyList<string> Values { get; } = Array.Empty<string>();

    public override string ToString() => ((string[])[DbName, .. Values]).Join(';');

    public static implicit operator string(DbNameId dbNameId) => dbNameId.ToString();
    public static implicit operator DbNameId(string dbNameId) => new DbNameId(dbNameId);
    public static implicit operator DbNameId((string dbName, IEnumerable<string> contexts) dbNameId) => new DbNameId(dbNameId.dbName, dbNameId.contexts);

    public static Option<(string DbName, IReadOnlyList<string> Keys)> Parse(string? dbNameId)
    {
        if (dbNameId.IsEmpty()) return StatusCode.BadRequest;
        if (dbNameId == "*") return (dbNameId, Array.Empty<string>());

        var parts = dbNameId.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        Option<(string dbName, IReadOnlyList<string> contexts)> result = parts switch
        {
            { Length: > 0 } v => v.All(x => IdPatterns.IsName(x)) switch
            {
                false => (StatusCode.BadRequest, "Invalid character(s)"),
                true => (v[0], v.Skip(1).ToArray()),
            },
            _ => StatusCode.BadRequest,
        };

        return result;
    }
}
