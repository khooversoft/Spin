using Azure;
using Directory.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Service;

public class DirectoryEntryBuilder
{
    public DirectoryEntryBuilder() { }

    public DirectoryEntryBuilder(DirectoryEntry directoryEntry)
    {
        DirectoryId = (DirectoryId)directoryEntry.DirectoryId;
        ClassType = directoryEntry.ClassType;
        ETag = directoryEntry.ETag;

        directoryEntry.Properties.Values.ForEach(x => Properties.Add(x.Name, x));
    }

    public DirectoryId? DirectoryId { get; set; }
    public string? ClassType { get; set; }
    public ETag? ETag { get; set; }
    public IDictionary<string, EntryProperty> Properties { get; } = new Dictionary<string, EntryProperty>(StringComparer.OrdinalIgnoreCase);

    public DirectoryEntryBuilder SetDirectoryId(DirectoryId directoryId) => this.Action(x => x.DirectoryId = directoryId);
    public DirectoryEntryBuilder SetClassType(string classType) => this.Action(x => x.ClassType = classType);
    public DirectoryEntryBuilder SetETag(ETag eTag) => this.Action(x => x.ETag = eTag);

    public DirectoryEntryBuilder AddProperty(params EntryProperty[] entryProperties) => this.Action(x => entryProperties.ForEach(x => Properties.Add(x.Name, x)));

    public DirectoryEntryBuilder AddProperty<T>(T subject) where T : class
    {
        IReadOnlyList<KeyValuePair<string, string>> classProperties = subject.GetConfigurationValues();

        var properties = classProperties
            .Select(x => new EntryProperty
            {
                Name = x.Key,
                Value = x.Value,
                IsSecret = false
            }).ToArray();

        AddProperty(properties);

        return this;
    }

    public DirectoryEntry Build()
    {
        DirectoryId.VerifyNotNull($"{nameof(DirectoryId)} is required");
        ClassType.VerifyNotEmpty($"{nameof(ClassType)} is required");

        return new DirectoryEntry
        {
            DirectoryId = (string)DirectoryId,
            ClassType = ClassType,
            ETag = ETag,
            Properties = Properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
        };
    }
}
