namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents a Git blob object handler.
/// </summary>
public interface IBlob
{
    /// <summary>
    /// Writes blob content to the object store.
    /// </summary>
    /// <param name="content">The content to write.</param>
    /// <returns>The SHA-1 hash of the blob.</returns>
    string? Write(byte[]? content);

    /// <summary>
    /// Reads blob content from the object store.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the blob.</param>
    /// <returns>The blob content.</returns>
    byte[]? Read(string hash);
}
