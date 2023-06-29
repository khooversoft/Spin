using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Application;

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
        var objectIdOption = grainId.ToObjectIdIfValid();
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
