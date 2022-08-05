
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Service;

public static class BlockTypeRequestExtensions
{
    public static BlockTypeRequest ToBlockTypeRequest(this Type type) => new BlockTypeRequest() + type;
    public static BlockTypeRequest ToBlockTypeRequest(this (Type type, bool all) subject) => new BlockTypeRequest() + (subject.type, subject.all);
}


public class BlockTypeRequest : IEnumerable<(string BlockType, bool All)>
{
    private enum SearchTypeRequest
    {
        Latest,
        All,
    }

    private readonly List<KeyValuePair<string, SearchTypeRequest>> _list;

    public BlockTypeRequest() => _list = new();
    private BlockTypeRequest(IEnumerable<KeyValuePair<string, SearchTypeRequest>> list) => _list = list.ToList();

    public BlockTypeRequest Add<T>(bool all = false) => Add(typeof(T), all);

    public BlockTypeRequest Add(Type type, bool all = false)
    {
        type.NotNull();

        _list.Add(new KeyValuePair<string, SearchTypeRequest>(type.GetTypeName(), all ? SearchTypeRequest.All : SearchTypeRequest.Latest));
        return this;
    }

    public override string ToString() => _list
        .Select(x => x.Key + (x.Value == SearchTypeRequest.All ? ".*" : String.Empty))
        .Join(";")
        .ToNullIfEmpty() ?? "*";

    public IEnumerator<(string BlockType, bool All)> GetEnumerator() => _list
        .Select(x => (x.Key, x.Value == SearchTypeRequest.All))
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static BlockTypeRequest operator +(BlockTypeRequest builder, Type type) => builder.Add(type);
    public static BlockTypeRequest operator +(BlockTypeRequest builder, (Type type, bool all) data) => builder.Add(data.type, data.all);

    public static explicit operator BlockTypeRequest(Type type) => new BlockTypeRequest().Add(type);
    public static explicit operator BlockTypeRequest((Type type, bool all) data) => new BlockTypeRequest().Add(data.type, data.all);
    public static implicit operator string(BlockTypeRequest blockTypeRequest) => blockTypeRequest.ToString();

    public static BlockTypeRequest Parse(string search)
    {
        search.NotEmpty();

        if (search == "*") return new BlockTypeRequest();

        List<KeyValuePair<string, SearchTypeRequest>> list = search
            .Split(';')
            .Select(x => x.Split('.').Func(y => new KeyValuePair<string, SearchTypeRequest>(
                y.First(),
                y.Skip(1).FirstOrDefault()?.Equals("*") == true ? SearchTypeRequest.All : SearchTypeRequest.Latest
            )))
            .ToList();

        return new BlockTypeRequest(list);
    }
}
