using System.Collections.Generic;

namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents a Git tag object.
/// </summary>
public interface ITag
{
    /// <summary>
    /// Writes a tag object to the repository.
    /// </summary>
    /// <param name="tag">The tag data to write.</param>
    /// <returns>The SHA-1 hash of the tag object, or null if the operation fails.</returns>
    string? Write(TagData? tag);

    /// <summary>
    /// Reads a tag object from the repository.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the tag to read.</param>
    /// <returns>The tag data, or null if the tag doesn't exist or is invalid.</returns>
    TagData? Read(string hash);
}

/// <summary>
/// Represents tag data.
/// </summary>
public class TagData
{
    /// <summary>
    /// The object being tagged (commit, tree, blob, or another tag).
    /// </summary>
    public string Object { get; set; }

    /// <summary>
    /// The type of the object being tagged (commit, tree, blob, tag).
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The tag name.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// The tagger information.
    /// </summary>
    public AuthorInfo? Tagger { get; set; }

    /// <summary>
    /// The tag message.
    /// </summary>
    public string Message { get; set; }

    public TagData()
    {
        Object = string.Empty;
        Type = string.Empty;
        Tag = string.Empty;
        Message = string.Empty;
    }

    public TagData(string @object, string type, string tag, AuthorInfo? tagger, string message)
    {
        Object = @object;
        Type = type;
        Tag = tag;
        Tagger = tagger;
        Message = message;
    }
}
