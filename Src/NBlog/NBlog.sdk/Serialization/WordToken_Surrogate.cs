using Toolbox.DocumentSearch;

namespace NBlog.sdk.Serialization;

[GenerateSerializer]
public struct WordToken_Surrogate
{
    public string Word;
    public int Weight;
}


[RegisterConverter]
public sealed class WordTokenConverter : IConverter<WordToken, WordToken_Surrogate>
{
    public WordToken ConvertFromSurrogate(in WordToken_Surrogate surrogate) => new WordToken(surrogate.Word, surrogate.Weight);

    public WordToken_Surrogate ConvertToSurrogate(in WordToken value) => new WordToken_Surrogate
    {
        Word = value.Word,
        Weight = value.Weight,
    };
}