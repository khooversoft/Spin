using System.Collections.Generic;
using System.IO;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class DirectoryEntryBuilder
{
    private readonly List<string> _files = new List<string>();

    public DirectoryEntryBuilder() { }

    public IList<string> Files => _files;

    public DirectoryEntryBuilder Add(string file) => this.Action(x => x._files.Add(file));

    public DirectoryEntryBuilder Add(IEnumerable<string> files) => this.Action(x => x._files.AddRange(files));

    public IReadOnlyList<DirectoryEntry> Build()
    {
        Files.Count.VerifyAssert(x => x > 0, "No files specified");

        List<DirectoryEntry> list = new();
        foreach (string file in Files)
        {
            string json = File.ReadAllText(file);

            IList<DirectoryEntry> readList = Json.Default.Deserialize<IList<DirectoryEntry>>(json)
                .VerifyNotNull($"Cannot read json file {file}");

            list.AddRange(readList);
        }

        return list;
    }
}
