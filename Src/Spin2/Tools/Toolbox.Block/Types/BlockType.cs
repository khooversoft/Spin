using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace Toolbox.Block;

public readonly record struct BlockType
{
    public BlockType(string name) => Value = name.Assert(x => IsValid(x), "Syntax error");

    public string Value { get; }
    public override string ToString() => Value;

    public static bool IsValid(string subject) => IdPatterns.IsBlockType(subject);

    public static bool operator ==(BlockType left, string right) => left.Value.Equals(right);
    public static bool operator !=(BlockType left, string right) => !(left == right);
    public static bool operator ==(string left, BlockType right) => left.Equals(right.Value);
    public static bool operator !=(string left, BlockType right) => !(left == right);

    public static implicit operator BlockType(string subject) => new BlockType(subject);
    public static implicit operator string(BlockType subject) => subject.ToString();

    public static string Verify(string id) => id.Action(x => BlockType.IsValid(x).Assert($"{x} is not valid name id"));
}
