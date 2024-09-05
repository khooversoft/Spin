﻿using Toolbox.Graph;

namespace Toolbox.Orleans.Serialization;

[GenerateSerializer]
public struct GraphDataLink_Surrogate
{
    [Id(0)] public string Name;
    [Id(1)] public string FileId;
}

[RegisterConverter]
public sealed class GraphDataLink_SurrogateConverter : IConverter<GraphLink, GraphDataLink_Surrogate>
{
    public GraphLink ConvertFromSurrogate(in GraphDataLink_Surrogate surrogate) => new GraphLink
    {
        Name = surrogate.Name,
        FileId = surrogate.FileId,
    };

    public GraphDataLink_Surrogate ConvertToSurrogate(in GraphLink value) => new GraphDataLink_Surrogate
    {
        Name = value.Name,
        FileId = value.FileId,
    };
}