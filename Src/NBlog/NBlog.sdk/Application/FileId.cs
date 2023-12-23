using Toolbox.Extensions;
using Toolbox.Types;

namespace NBlog.sdk;

public record FileId
{
    private FileId(string id) => Id = id;
    public string Id { get; }

    public override string ToString() => Id;

    public static explicit operator string(FileId articleId) => articleId.ToString();
    public static explicit operator FileId(string id) => FileId.Create(id).ThrowOnError().Return();

    public static Option<FileId> Create(string id)
    {
        if (id.IsEmpty()) return StatusCode.BadRequest;

        if (id.Length > 2 && !id.All(y => char.IsLetterOrDigit(y) || y == '.' || y == '/' || y == '-')) return (StatusCode.BadRequest, "Must be letter, number, '.', '/', or '-'");
        if (id.IndexOf("..") >= 0) return (StatusCode.BadRequest, "Cannot have '..'");
        if (id.IndexOf("//") >= 0) return (StatusCode.BadRequest, "Cannot have '//'");

        var parts = id.Split('/');
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (!char.IsLetter(part[0])) return (StatusCode.BadRequest, "Must start with letter");
            if (part.Length > 1 && !char.IsLetterOrDigit(part[^1])) return (StatusCode.BadRequest, "Must end with letter or number");
        }

        return new FileId(id);
    }

    public static bool Validate(string id, out Option result)
    {
        result = FileId.Create(id).ToOptionStatus();
        return result.IsOk();
    }
}
