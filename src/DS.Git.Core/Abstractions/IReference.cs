namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents Git reference operations.
/// </summary>
public interface IReference
{
    /// <summary>
    /// Creates or updates a reference to point to a commit/object.
    /// </summary>
    /// <param name="refName">The reference name (e.g., "refs/heads/main", "HEAD").</param>
    /// <param name="hash">The SHA-1 hash the reference should point to.</param>
    /// <returns>True if successful, false otherwise.</returns>
    bool UpdateRef(string refName, string hash);

    /// <summary>
    /// Reads a reference and returns the hash it points to.
    /// </summary>
    /// <param name="refName">The reference name (e.g., "refs/heads/main", "HEAD").</param>
    /// <returns>The SHA-1 hash the reference points to, or null if not found.</returns>
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
    /// Creates or updates a symbolic reference (like HEAD pointing to a branch).
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
}
