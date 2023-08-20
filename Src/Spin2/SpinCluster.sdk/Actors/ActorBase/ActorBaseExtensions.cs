using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Orleans.Types;

public static class ActorBaseExtensions
{
    public static async Task<Option> Delete<T>(this IPersistentState<T> state, string actorKey, ScopeContextLocation location)
    {
        location.LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), actorKey);

        await state.ClearStateAsync();
        return new Option(StatusCode.OK);
    }

    public static Task<Option<T>> Get<T>(this IPersistentState<T> state, string actorKey, ScopeContextLocation location)
    {
        location.LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), actorKey);

        return state.RecordExists switch
        {
            false => Task.FromResult(new Option<T>(StatusCode.NotFound)),
            true => Task.FromResult((Option<T>)state.State),
        };
    }

    public static async Task<Option> Set<T>(this IPersistentState<T> state, T model, string actorKey, IValidator<T> validator, ScopeContextLocation location)
    {
        location.LogInformation("Setting {typeName}, id={id}, model={model}", typeof(T).GetTypeName(), actorKey, model.ToJsonPascalSafe(location.Context));

        var validatorResult = validator.Validate(model).LogResult(location);
        if (validatorResult.IsError()) return validatorResult.ToOptionStatus();

        state.State = model;
        await state.WriteStateAsync();

        return new Option(StatusCode.OK);
    }

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
