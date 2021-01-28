namespace Toolbox.BlockDocument
{
    public class BlockChainNodeModel
    {
        public int Index { get; set; }

        public string? PreviousHash { get; set; }

        public string? Hash { get; set; }

        public DataBlockModel<BlockBlobModel>? Blob { get; set; }

        public DataBlockModel<HeaderBlockModel>? Header { get; set; }

        public DataBlockModel<TrxBlockModel>? Trx { get; set; }

        public DataBlockModel<TextBlockModel>? Text { get; set; }
    }
}
