using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinCluster.sdk.Models;

public readonly record struct ObjectIdInfo
{
    public ObjectIdInfo(ObjectId objectId, string filePath)
    {
        ObjectId = objectId;
        FilePath = filePath;
    }

    public void Deconstruct(out ObjectId ObjectId, out string FilePath)
    {
        ObjectId = this.ObjectId;
        FilePath = this.FilePath;
    }

    public ObjectId ObjectId { get; }
    public string FilePath { get; }

    public static Option<ObjectIdInfo> Parse(string grainId, ScopeContext context)
    {
        var objectIdOption = ObjectId.Create(grainId).LogResult(context.Location());
        if (objectIdOption.IsError())
        {
            context.Location().LogError("Invalid GrainId, id={id}", grainId);
            return new Option<ObjectIdInfo>(StatusCode.BadRequest, $"Invalid GrainId={grainId}");
        }

        ObjectId objectId = objectIdOption.Return();
        string filePath = objectId.Tenant + "/" + objectId.Path;

        context.Location().LogInformation("GrainId={grainId} to FilePath={filePath}", grainId.ToString(), filePath);

        return new ObjectIdInfo(objectId, filePath);
    }
}
