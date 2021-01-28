namespace Toolbox.BlockDocument
{
    public record TrxBlockModel : IDataBlockModelType
    {
        public string? ReferenceId { get; init; }

        public string? TransactionType { get; init; }

        public long Value { get; init; }
    }
}
