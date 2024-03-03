using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Identity;

public class UserAccess
{
    private readonly GraphMap _map;
    public UserAccess(GraphMap map) => _map = map.NotNull();

    public Option Delete(string id) => _map.Nodes.Remove(ToKey(id)) ? StatusCode.OK : StatusCode.NotFound;

    public Option<IdentityUser> Get(string id)
    {
        var node = _map.Nodes.Get($"user:{id}");
        if (node.IsError()) return node.ToOptionStatus<IdentityUser>();

        var user = node.Return().Tags.ToObject<IdentityUser>();
        if( user.Validate().IsError(out Option v)) return v.ToOptionStatus<IdentityUser>();

        return user;
    }

    public Option Set(IdentityUser user, string? tags = null)
    {
        if (user.Validate().IsError(out Option v)) return v;

        var t1 = new Tags()
            .SetObject(user)
            .Set(tags);

        var node = new GraphNode($"user:{user.Id}", t1.ToString());
        _map.Nodes.Add(node, true).ThrowOnError();

        return StatusCode.OK;
    }

    private string ToKey(string id) => $"user:{id.NotEmpty().ToLower()}";
}
