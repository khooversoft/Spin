using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public class DirectoryId
{
    public DirectoryId(string id)
    {
        id.VerifyNotEmpty(id);

        Id = id.ToLower();
        VerifyId(Id);
    }

    public string Id { get; }

    public string Domain => Id.Split('/')[0];

    public string Path => Id.Split('/').Skip(1).Join("'/");

    public IReadOnlyList<string> PathItems => Id.Split('/').Skip(1).ToArray();


    //  ///////////////////////////////////////////////////////////////////////////////////////////

    public override string ToString() => Id;

    public override bool Equals(object? obj) => obj is DirectoryId id && Id == id.Id;

    public override int GetHashCode() => HashCode.Combine(Id);


    // ////////////////////////////////////////////////////////////////////////////////////////////

    public static explicit operator DirectoryId(string id) => new DirectoryId(id);

    public static explicit operator string(DirectoryId articleId) => articleId.ToString();

    public static bool operator ==(DirectoryId? left, DirectoryId? right) => EqualityComparer<DirectoryId>.Default.Equals(left, right);

    public static bool operator !=(DirectoryId? left, DirectoryId? right) => !(left == right);

    public static void VerifyId(string id)
    {
        id.Split('/')
            .VerifyAssert(x => x.Length > 1, "Missing domain or id (ex: domain/subject)")
            .VerifyAssert(x => x.All(y => !y.IsEmpty()), "path part is empty")
            .ForEach(x =>
            {
                x.VerifyAssert(x => char.IsLetterOrDigit(x[0]), $"{x} Must start with letter or number");
                x.VerifyAssert(x => char.IsLetterOrDigit(x[^1]), "Must end with letter or number");
                x.IsDirectoryIdValid().VerifyAssert(x => x.Valid, x => x.Message);
            });
    }
}


public static class DirectoryIdUtility
{
    private const string _extension = ".json";

    public static string ToUrlEncoding(this DirectoryId directoryId) => directoryId.Id.Replace('/', ':');

    public static DirectoryId FromUrlEncoding(string id) => new DirectoryId(id.Replace(':', '/'));

    public static string ToFileName(this DirectoryId directoryId) => directoryId.Id + _extension;

    public static string FromFileName(string filename) => filename.EndsWith(_extension) ? filename[0..^_extension.Length] : filename;
}
