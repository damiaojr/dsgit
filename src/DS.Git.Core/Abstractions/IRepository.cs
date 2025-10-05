namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents a Git repository with core operations.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Initializes a new Git repository at the specified path.
    /// </summary>
    /// <param name="path">The path where the repository should be initialized.</param>
    /// <returns>True if successful, false otherwise.</returns>
    bool Init(string path);

    /// <summary>
    /// Writes a blob object to the repository.
    /// </summary>
    /// <param name="content">The content to store as a blob.</param>
    /// <returns>The SHA-1 hash of the blob, or null if the operation failed.</returns>
    string? WriteBlob(byte[]? content);

    /// <summary>
    /// Reads a blob object from the repository.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the blob to read.</param>
    /// <returns>The blob content, or null if not found.</returns>
    byte[]? ReadBlob(string hash);

    /// <summary>
    /// Writes a tree object to the repository.
    /// </summary>
    /// <param name="entries">The tree entries (files and subdirectories).</param>
    /// <returns>The SHA-1 hash of the tree, or null if the operation failed.</returns>
    string? WriteTree(IEnumerable<TreeEntry>? entries);

    /// <summary>
    /// Reads a tree object from the repository.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the tree to read.</param>
    /// <returns>The tree entries, or null if not found.</returns>
    IEnumerable<TreeEntry>? ReadTree(string hash);

    /// <summary>
    /// Writes a commit object to the repository.
    /// </summary>
    /// <param name="commit">The commit data to write.</param>
    /// <returns>The SHA-1 hash of the commit, or null if the operation failed.</returns>
    string? WriteCommit(CommitData? commit);

    /// <summary>
    /// Reads a commit object from the repository.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the commit to read.</param>
    /// <returns>The commit data, or null if not found.</returns>
    CommitData? ReadCommit(string hash);

    /// <summary>
    /// Writes a tag object to the repository.
    /// </summary>
    /// <param name="tag">The tag data to write.</param>
    /// <returns>The SHA-1 hash of the tag, or null if the operation failed.</returns>
    string? WriteTag(TagData? tag);

    /// <summary>
    /// Reads a tag object from the repository.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the tag to read.</param>
    /// <returns>The tag data, or null if not found.</returns>
    TagData? ReadTag(string hash);

    /// <summary>
    /// Creates or updates a reference.
    /// </summary>
    /// <param name="refName">The reference name (e.g., "refs/heads/main", "HEAD").</param>
    /// <param name="hash">The SHA-1 hash the reference should point to.</param>
    /// <returns>True if successful, false otherwise.</returns>
    bool UpdateRef(string refName, string hash);

    /// <summary>
    /// Reads a reference and returns the hash it points to.
    /// </summary>
    /// <param name="refName">The reference name.</param>
    /// <returns>The hash or symbolic ref the reference points to, or null if not found.</returns>
    string? ReadRef(string refName);

    /// <summary>
    /// Deletes a reference.
    /// </summary>
    /// <param name="refName">The reference name to delete.</param>
    /// <returns>True if successful, false otherwise.</returns>
    bool DeleteRef(string refName);

    /// <summary>
    /// Lists all references matching a pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match (e.g., "refs/heads/", "refs/tags/").</param>
    /// <returns>Dictionary of reference names and their hashes.</returns>
    Dictionary<string, string> ListRefs(string? pattern = null);

    /// <summary>
    /// Creates or updates a symbolic reference.
    /// </summary>
    /// <param name="refName">The symbolic reference name (e.g., "HEAD").</param>
    /// <param name="targetRef">The target reference (e.g., "refs/heads/main").</param>
    /// <returns>True if successful, false otherwise.</returns>
    bool UpdateSymbolicRef(string refName, string targetRef);

    /// <summary>
    /// Resolves a reference to its final hash (following symbolic refs).
    /// </summary>
    /// <param name="refName">The reference name to resolve.</param>
    /// <returns>The final SHA-1 hash, or null if not found.</returns>
    string? ResolveRef(string refName);

    /// <summary>
    /// Checks if a reference exists.
    /// </summary>
    /// <param name="refName">The reference name to check.</param>
    /// <returns>True if the reference exists, false otherwise.</returns>
    bool RefExists(string refName);

    /// <summary>
    /// Gets the repository path.
    /// </summary>
    string? RepositoryPath { get; }
}
