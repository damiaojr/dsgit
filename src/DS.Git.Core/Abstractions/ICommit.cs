namespace DS.Git.Core.Abstractions;

/// <summary>
/// Represents a Git commit object handler.
/// </summary>
public interface ICommit
{
    /// <summary>
    /// Writes a commit object to the object store.
    /// </summary>
    /// <param name="commit">The commit data to write.</param>
    /// <returns>The SHA-1 hash of the commit.</returns>
    string? Write(CommitData? commit);

    /// <summary>
    /// Reads a commit object from the object store.
    /// </summary>
    /// <param name="hash">The SHA-1 hash of the commit.</param>
    /// <returns>The commit data.</returns>
    CommitData? Read(string hash);
}

/// <summary>
/// Represents the data contained in a Git commit object.
/// </summary>
public class CommitData
{
    /// <summary>
    /// SHA-1 hash of the tree object for this commit.
    /// </summary>
    public string Tree { get; set; } = string.Empty;

    /// <summary>
    /// SHA-1 hashes of parent commits (empty for initial commit).
    /// </summary>
    public List<string> Parents { get; set; } = new();

    /// <summary>
    /// Author information.
    /// </summary>
    public AuthorInfo Author { get; set; } = new();

    /// <summary>
    /// Committer information.
    /// </summary>
    public AuthorInfo Committer { get; set; } = new();

    /// <summary>
    /// Commit message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public CommitData() { }

    public CommitData(string tree, List<string> parents, AuthorInfo author, AuthorInfo committer, string message)
    {
        Tree = tree;
        Parents = parents ?? new List<string>();
        Author = author ?? new AuthorInfo();
        Committer = committer ?? new AuthorInfo();
        Message = message ?? string.Empty;
    }
}

/// <summary>
/// Represents author/committer information in a commit.
/// </summary>
public class AuthorInfo
{
    /// <summary>
    /// Author/committer name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Author/committer email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp (Unix epoch seconds).
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Timezone offset (e.g., "+0000").
    /// </summary>
    public string Timezone { get; set; } = "+0000";

    public AuthorInfo() { }

    public AuthorInfo(string name, string email, long timestamp, string timezone)
    {
        Name = name;
        Email = email;
        Timestamp = timestamp;
        Timezone = timezone;
    }

    /// <summary>
    /// Returns the formatted author/committer line for Git commit format.
    /// </summary>
    public string ToGitFormat()
    {
        return $"{Name} <{Email}> {Timestamp} {Timezone}";
    }
}