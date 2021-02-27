﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.BlockDocument
{
    public class BlobBlock : BlockBase, IBlockType
    {
        public BlobBlock(string name, string contentType, string author, IReadOnlyList<byte> content, IEnumerable<KeyValuePair<string, string>>? properties = null)
            : base(properties)
        {
            name.VerifyNotEmpty(nameof(name));
            contentType.VerifyNotEmpty(nameof(contentType));
            author.VerifyNotEmpty(nameof(author));
            content.VerifyNotNull(nameof(content))
                .VerifyAssert(x => x.Count > 0, "Content must have content");

            Name = name;
            ContentType = contentType;
            Author = author;
            Content = content.ToArray();

            Digest = GetDigest();
        }

        public string Name { get; }

        public string ContentType { get; }

        public string Author { get; }

        public IReadOnlyList<byte> Content { get; }

        public string Digest { get; }

        public string GetDigest() => $"{Name}-{ContentType}-{Author}-" + base.ToString()
                .ToBytes()
                .Concat(Content)
                .ToSHA256Hash();

        public override bool Equals(object? obj)
        {
            return obj is BlobBlock blobBlock &&
                Name == blobBlock.Name &&
                ContentType == blobBlock.ContentType &&
                Author == blobBlock.Author &&
                Enumerable.SequenceEqual(Content, blobBlock.Content) &&
                Digest == blobBlock.Digest &&
                base.Equals(blobBlock);
        }

        public override int GetHashCode() => HashCode.Combine(Name, ContentType, Author, Content, Digest);

        public static bool operator ==(BlobBlock v1, BlobBlock v2) => v1.Equals(v2);

        public static bool operator !=(BlobBlock v1, BlobBlock v2) => !v1.Equals(v2);
    }
}
