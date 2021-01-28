namespace Toolbox.BlockDocument
{
    public record TextBlockModel : IDataBlockModelType
    {
        public string? Name { get; init; }

        public string? ContentType { get; init; }

        public string? Author { get; init; }

        public string? Content { get; init; }
    }
}
