using System;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.BlockDocument
{
    public class TextBlock : BlockBase, IBlockType
    {
        public TextBlock(string name, string contentType, string author, string content, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : base(properties)
        {
            name.VerifyNotEmpty(nameof(name));
            contentType.VerifyNotEmpty(nameof(contentType));
            author.VerifyNotEmpty(nameof(author));
            content.VerifyNotEmpty(nameof(content));

            Name = name.Trim();
            ContentType = contentType.Trim();
            Author = author.Trim();
            Content = content.Trim();

            Digest = GetDigest();
        }

        public string Name { get; }

        public string ContentType { get; }

        public string Author { get; }

        public string Content { get; }

        public string Digest { get; }

        public string GetDigest() => $"{Name}-{ContentType}-{Author}-{Content}" + base.ToString()
                .ToBytes()
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            return obj is TextBlock textBlock &&
                Name == textBlock.Name &&
                ContentType == textBlock.ContentType &&
                Author == textBlock.Author &&
                Content == textBlock.Content &&
                Digest == textBlock.Digest &&
                base.Equals(textBlock);
        }

        public override int GetHashCode() => HashCode.Combine(Name.Length, ContentType, Author, Content, Digest);

        public static bool operator ==(TextBlock v1, TextBlock v2) => v1.Equals(v2);

        public static bool operator !=(TextBlock v1, TextBlock v2) => !v1.Equals(v2);
    }
}