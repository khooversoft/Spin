namespace Toolbox.BlockDocument
{
    public record BlockChainNodeModel
    {
        public int Index { get; init; }

        public string? PreviousHash { get; init; }

        public string? Hash { get; init; }

        public DataBlockModel<BlockBlobModel>? Blob { get; init; }

        public DataBlockModel<HeaderBlockModel>? Header { get; init; }

        public DataBlockModel<TrxBlockModel>? Trx { get; init; }

        public DataBlockModel<TextBlockModel>? Text { get; init; }
    }
}
