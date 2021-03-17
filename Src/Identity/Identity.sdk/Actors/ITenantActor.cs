using Identity.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor;

namespace Identity.sdk.Actors
{
    public interface ITenantActor : IActor
    {
        Task<bool> Delete(CancellationToken token);

        Task<Tenant?> Get(CancellationToken token);

        Task Set(Tenant tenant, CancellationToken token);

    }
}
