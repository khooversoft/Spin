namespace Toolbox.BlockDocument
{
    public class TextBlockModel : IDataBlockModelType
    {
        public string? Name { get; set; }

        public string? ContentType { get; set; }

        public string? Author { get; set; }

        public string? Content { get; set; }
    }
}
