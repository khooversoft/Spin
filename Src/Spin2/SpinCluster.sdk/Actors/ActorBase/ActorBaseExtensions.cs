using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;
using Toolbox.Orleans.Types;

namespace Toolbox.Orleans.Types;

public static class ActorBaseExtensions
{
    public static async Task<SpinResponse> Delete<T>(this IPersistentState<T> state, string actorKey, ScopeContextLocation location)
    {
        location.LogInformation("Deleting {typeName}, id={id}", typeof(T).GetTypeName(), actorKey);

        await state.ClearStateAsync();
        return new SpinResponse(StatusCode.OK);
    }

    public static Task<SpinResponse<T>> Get<T>(this IPersistentState<T> state, string actorKey, ScopeContextLocation location)
    {
        location.LogInformation("Getting {typeName}, id={id}", typeof(T).GetTypeName(), actorKey);

        return state.RecordExists switch
        {
            false => Task.FromResult(new SpinResponse<T>(StatusCode.NotFound)),
            true => Task.FromResult((SpinResponse<T>)state.State),
        };
    }

    public static async Task<SpinResponse> Set<T>(this IPersistentState<T> state, T model, string actorKey, IValidator<T> validator, ScopeContextLocation location)
    {
        location.LogInformation("Setting {typeName}, id={id}, model={model}", typeof(T).GetTypeName(), actorKey, model.ToJsonPascalSafe(location.Context));

        ValidatorResult validatorResult = validator.Validate(model);
        if (!validatorResult.IsValid)
        {
            location.LogError(validatorResult.FormatErrors());
            return new SpinResponse(StatusCode.BadRequest, validatorResult.FormatErrors());
        }

        state.State = model;
        await state.WriteStateAsync();

        return new SpinResponse(StatusCode.OK);
    }

    public static void VerifySchema(this Grain grain, string schema, ScopeContext context)
    {
        string actorKey = grain.GetPrimaryKeyString();
        Option<ObjectId> objectId = ObjectId.CreateIfValid(actorKey).LogResult(context.Location());

        if (objectId.IsError()) throw new ArgumentException($"Invalid object Id, actorKey={actorKey}, error={objectId.Error}");

        objectId.Return().Schema.Assert(x => x == schema, x => $"Invalid schema, {x} does not match {schema}");
    }
}
