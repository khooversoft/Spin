namespace Toolbox.BlockDocument
{
    public class TrxBlockModel : IDataBlockModelType
    {
        public string? ReferenceId { get; set; }

        public string? TransactionType { get; set; }

        public long Value { get; set; }
    }
}
