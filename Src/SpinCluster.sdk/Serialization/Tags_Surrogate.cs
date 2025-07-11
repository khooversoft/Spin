//using Toolbox.Types;

//namespace SpinCluster.sdk.Serialization;

//[GenerateSerializer]
//public struct Tags_Surrogate
//{
//    [Id(0)] public IReadOnlyList<KeyValuePair<string, string?>> Tags;
//}


//[RegisterConverter]
//public sealed class Tags_SurrogateConverter : IConverter<Tags, Tags_Surrogate>
//{
//    public Tags ConvertFromSurrogate(in Tags_Surrogate surrogate) => new Tags(surrogate.Tags);

//    public Tags_Surrogate ConvertToSurrogate(in Tags value) => new Tags_Surrogate
//    {
//        Tags = value.ToArray(),
//    };
//}
