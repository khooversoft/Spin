using Toolbox.Types;

namespace SpinCluster.sdk.Serialization;

[GenerateSerializer]
public struct ResourceIdSerialization
{
    [Id(0)] public string Id;
}

[RegisterConverter]
public sealed class ResourceIdSerializationConverter : IConverter<ResourceId, ResourceIdSerialization>
{
    public ResourceId ConvertFromSurrogate(in ResourceIdSerialization surrogate) => new ResourceId(surrogate.Id);

    public ResourceIdSerialization ConvertToSurrogate(in ResourceId value) => new ResourceIdSerialization
    {
        Id = value.Id,
    };
}