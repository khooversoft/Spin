using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Block.Model;

public sealed record FileBlock
{
    public static string BlockType { get; } = typeof(FileBlock).GetTypeName();

    public string Type { get; init; } = BlockType;
    public string FileId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string FileType { get; init; } = null!;  // Document type identifies how to read/write the file
    public byte[] Content { get; init; } = null!;
    public string? Tags { get; init; }
}


public static class FileBlockValidator
{
    public static IValidator<FileBlock> Validator { get; } = new Validator<FileBlock>()
        .RuleFor(x => x.FileId).NotEmpty()
        .RuleFor(x => x.FileType).ValidName()
        .RuleFor(x => x.Content).NotNull("No content").Must(x => x.Length > 0, _ => "Size error")
        .Build();
}

public class FileBlockBuilder
{
    private string _fileId = Guid.NewGuid().ToString();
    private string _fileType = null!;
    private byte[] _content = null!;
    private string? _tags = null!;

    public FileBlockBuilder SetFileId(string fileId) => this.Action(_ => _fileId = fileId);
    public FileBlockBuilder SetFileType(string fileType) => this.Action(_ => _fileType = fileType);
    public FileBlockBuilder SetContent(byte[] content) => this.Action(_ => _content = content);
    public FileBlockBuilder SetTags(string tags) => this.Action(_ => _tags = tags);

    public FileBlock Build()
    {
        _fileId.NotEmpty();
        _fileType.NotEmpty();
        _content.NotNull().Assert(x => x.Length > 0, "Size error");

        return new FileBlock
        {
            FileId = _fileId,
            FileType = _fileType,
            Content = _content,
            Tags = _tags
        };
    }
}