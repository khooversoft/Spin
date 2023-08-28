using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors;

public static class ActorExtensions
{
    public static void VerifySchema(this Grain grain, string schema, ScopeContext context)
    {
        string actorKey = grain.GetPrimaryKeyString();

        ResourceId resourceId = ResourceId.Create(actorKey).LogResult(context.Location()).ThrowOnError().Return();
        resourceId.Schema.Assert(x => x == schema, x => $"Invalid schema, {x} does not match {schema}");
    }

    public static Option VerifyIdentity(this Grain grain, string keyToMatch)
    {
        string actorKey = grain.GetPrimaryKeyString();

        if (!actorKey.EqualsIgnoreCase(keyToMatch))
        {
            return new Option(StatusCode.BadRequest, $"Key {keyToMatch} does not match actor id={actorKey}");
        }

        return StatusCode.OK;
    }
}
