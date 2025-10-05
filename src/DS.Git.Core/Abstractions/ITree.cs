namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents a Git tree object handler.
/// </summary>
public interface ITree
{
    /// <summary>
    /// Writes a tree object to the object store.
    /// </summary>
    /// <param name="entries">The tree entries (files and subdirectories).</param>
    /// <returns>The SHA-1 hash of the tree.</returns>
    string? Write(IEnumerable<TreeEntry>? entries);

    /// <summary>
    /// Reads a tree object from the object store.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the tree.</param>
    /// <returns>The tree entries.</returns>
    IEnumerable<TreeEntry>? Read(string hash);
}

/// <summary>
/// Represents an entry in a Git tree object.
/// </summary>
public class TreeEntry
{
    /// <summary>
    /// File mode (e.g., 100644 for regular file, 040000 for directory).
    /// </summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>
    /// Entry type (blob or tree).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// SHA-1 hash of the object.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Name of the file or directory.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public TreeEntry() { }

    public TreeEntry(string mode, string type, string hash, string name)
    {
        Mode = mode;
        Type = type;
        Hash = hash;
        Name = name;
    }
}
