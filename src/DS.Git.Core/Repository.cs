using System.IO;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Core;

/// <summary>
/// Represents a Git repository with core operations.
/// </summary>
public class Repository : IRepository
{
    private readonly ILogger<Repository>? _logger;
    private string? _repoPath;

    public string? RepositoryPath => _repoPath;

    public Repository() { }

    public Repository(ILogger<Repository> logger)
    {
        _logger = logger;
    }

    public bool Init(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // Ensure target directory exists
            Directory.CreateDirectory(path);

            // Create .git folder structure
            var gitDir = Path.Combine(path, ".git");
            Directory.CreateDirectory(gitDir);
            Directory.CreateDirectory(Path.Combine(gitDir, "objects"));
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "heads"));
            Directory.CreateDirectory(Path.Combine(gitDir, "refs", "tags"));

            // Create HEAD file only if it doesn't exist
            var headPath = Path.Combine(gitDir, "HEAD");
            if (!File.Exists(headPath))
            {
                File.WriteAllText(headPath, "ref: refs/heads/master\n");
            }

            // Create config file only if it doesn't exist
            var configPath = Path.Combine(gitDir, "config");
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath,
@"[core]
    repositoryformatversion = 0
    filemode = true
    bare = false
");
            }

            _repoPath = path;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? WriteBlob(byte[]? content)
    {
        if (_repoPath == null || content == null)
            return null;

        var blob = new Blob(_repoPath);
        return blob.Write(content);
    }

    public byte[]? ReadBlob(string hash)
    {
        if (_repoPath == null || string.IsNullOrWhiteSpace(hash))
            return null;

        var blob = new Blob(_repoPath);
        return blob.Read(hash);
    }

    public string? WriteTree(IEnumerable<TreeEntry>? entries)
    {
        if (_repoPath == null || entries == null)
            return null;

        var tree = new Tree(_repoPath);
        return tree.Write(entries);
    }

    public IEnumerable<TreeEntry>? ReadTree(string hash)
    {
        if (_repoPath == null || string.IsNullOrWhiteSpace(hash))
            return null;

        var tree = new Tree(_repoPath);
        return tree.Read(hash);
    }

    public string? WriteCommit(CommitData? commit)
    {
        if (_repoPath == null || commit == null)
            return null;

        var commitObj = new Commit(_repoPath);
        return commitObj.Write(commit);
    }

    public CommitData? ReadCommit(string hash)
    {
        if (_repoPath == null || string.IsNullOrWhiteSpace(hash))
            return null;

        var commitObj = new Commit(_repoPath);
        return commitObj.Read(hash);
    }

    public string? WriteTag(TagData? tag)
    {
        if (_repoPath == null || tag == null)
            return null;

        var tagObj = new Tag(_repoPath);
        return tagObj.Write(tag);
    }

    public TagData? ReadTag(string hash)
    {
        if (_repoPath == null || string.IsNullOrWhiteSpace(hash))
            return null;

        var tagObj = new Tag(_repoPath);
        return tagObj.Read(hash);
    }

    public bool UpdateRef(string refName, string hash)
    {
        if (_repoPath == null)
            return false;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.UpdateRef(refName, hash);
        }
        catch
        {
            return false;
        }
    }

    public string? ReadRef(string refName)
    {
        if (_repoPath == null)
            return null;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.ReadRef(refName);
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteRef(string refName)
    {
        if (_repoPath == null)
            return false;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.DeleteRef(refName);
        }
        catch
        {
            return false;
        }
    }

    public Dictionary<string, string> ListRefs(string? pattern = null)
    {
        if (_repoPath == null)
            return new Dictionary<string, string>();

        try
        {
            var reference = new Reference(_repoPath);
            return reference.ListRefs(pattern);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    public bool UpdateSymbolicRef(string refName, string targetRef)
    {
        if (_repoPath == null)
            return false;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.UpdateSymbolicRef(refName, targetRef);
        }
        catch
        {
            return false;
        }
    }

    public string? ResolveRef(string refName)
    {
        if (_repoPath == null)
            return null;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.ResolveRef(refName);
        }
        catch
        {
            return null;
        }
    }

    public bool RefExists(string refName)
    {
        if (_repoPath == null)
            return false;

        try
        {
            var reference = new Reference(_repoPath);
            return reference.RefExists(refName);
        }
        catch
        {
            return false;
        }
    }

    public static string? FindRepoPath(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}