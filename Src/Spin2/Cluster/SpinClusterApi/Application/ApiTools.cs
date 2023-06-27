using Toolbox.Types;

namespace SpinClusterApi.Application;

internal static class ApiTools
{
    public static Option<ObjectId> TestObjectId(string objectId, ScopeContextLocation location)
    {
        if (!ObjectId.IsValid(objectId))
        {
            location.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
            return new Option<ObjectId>(StatusCode.BadRequest);
        }

        return objectId.ToObjectId();
    }
}
