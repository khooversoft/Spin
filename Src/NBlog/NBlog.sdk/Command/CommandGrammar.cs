using Toolbox.LangTools;

namespace NBlog.sdk;

/// <summary>
/// 
/// Command syntax example: [{attr1};{attr2}] {fileId} = {filePath}
/// 
/// Attributes are optional
/// 
/// </summary>
public static class CommandGrammar
{
    public static ILangRoot ValueAssignment { get; } = new LsRoot(nameof(ValueAssignment))
        + new LsValue("fileId")
        + ("=", "equal")
        + new LsValue("localFilePath");

    public static ILangRoot Attributes { get; } = new LsRepeat(nameof(Attributes))
        + new LsValue("attributeName")
        + new LsToken(";", "delimiter", true);

    public static ILangRoot AttributeGroup { get; } = new LsRoot(nameof(AttributeGroup))
        + (new LsGroup("[", "]", "attribute-group") + Attributes);

    public static ILangRoot Root { get; } = new LsRoot()
        + (new LsOption("attribute-option") + AttributeGroup)
        + ValueAssignment;
}
