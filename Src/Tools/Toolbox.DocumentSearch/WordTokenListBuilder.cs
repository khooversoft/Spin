using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.DocumentSearch;

public class WordTokenListBuilder()
{
    private List<WordToken>? _wordList;

    public string? Json { get; set; }
    public WordTokenListBuilder SetJson(string json) => this.Action(x => x.Json = json);

    public List<WordToken> WordWeights => _wordList ??= new List<WordToken>();
    public WordTokenListBuilder Add(WordToken subject) => this.Action(x => x.WordWeights.Add(subject));
    public WordTokenListBuilder Add(string word, int weight) => this.Action(x => x.WordWeights.Add(new WordToken(word, weight)));

    public WordTokenList Build()
    {
        List<WordToken>? list = WordWeights;

        if (Json.IsNotEmpty())
        {
            var jsonList = Json.ToObject<List<WordToken>>().NotNull();

            list = list switch
            {
                null => jsonList,
                var v => v.Action(x => x.AddRange(jsonList)),
            };
        }

        return new WordTokenList(list);
    }
}
