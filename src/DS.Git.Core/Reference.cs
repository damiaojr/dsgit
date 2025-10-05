using System.Text.RegularExpressions;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;

namespace DS.Git.Core;

/// <summary>
/// Implements Git reference operations.
/// </summary>
public class Reference : IReference
{
    private readonly string _repoPath;
    private readonly string _gitDir;

    public Reference(string repoPath)
    {
        _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
        _gitDir = Path.Combine(_repoPath, ".git");

        if (!Directory.Exists(_gitDir))
        {
            throw new GitException($"Not a git repository: {_repoPath}");
        }
    }

    public bool UpdateRef(string refName, string hash)
    {
        if (string.IsNullOrWhiteSpace(refName) || string.IsNullOrWhiteSpace(hash))
            return false;

        if (!IsValidHash(hash))
            return false;

        try
        {
            var refPath = GetRefPath(refName);
            var refDir = Path.GetDirectoryName(refPath);
            
            if (refDir != null)
            {
                Directory.CreateDirectory(refDir);
            }

            File.WriteAllText(refPath, hash + "\n");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? ReadRef(string refName)
    {
        if (string.IsNullOrWhiteSpace(refName))
            return null;

        try
        {
            var refPath = GetRefPath(refName);
            if (!File.Exists(refPath))
                return null;

            var content = File.ReadAllText(refPath).Trim();
            
            // Handle symbolic refs (e.g., "ref: refs/heads/main")
            if (content.StartsWith("ref: "))
            {
                return content[5..]; // Return the symbolic reference as-is
            }

            return content;
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteRef(string refName)
    {
        if (string.IsNullOrWhiteSpace(refName))
            return false;

        // Don't allow deleting HEAD
        if (refName == "HEAD")
            return false;

        try
        {
            var refPath = GetRefPath(refName);
            if (!File.Exists(refPath))
                return false;

            File.Delete(refPath);

            // Clean up empty directories
            var refDir = Path.GetDirectoryName(refPath);
            if (refDir != null && Directory.Exists(refDir))
            {
                CleanEmptyDirectories(refDir);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Dictionary<string, string> ListRefs(string? pattern = null)
    {
        var refs = new Dictionary<string, string>();

        try
        {
            var refsDir = Path.Combine(_gitDir, "refs");
            if (!Directory.Exists(refsDir))
                return refs;

            // Collect all ref files
            var refFiles = Directory.GetFiles(refsDir, "*", SearchOption.AllDirectories);

            foreach (var refFile in refFiles)
            {
                var relativePath = Path.GetRelativePath(_gitDir, refFile);
                var refName = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                // Apply pattern filter if provided
                if (pattern != null && !refName.StartsWith(pattern.Replace('\\', '/')))
                    continue;

                var content = File.ReadAllText(refFile).Trim();
                
                // Skip symbolic refs in listing (or resolve them)
                if (!content.StartsWith("ref: "))
                {
                    refs[refName] = content;
                }
            }
        }
        catch
        {
            // Return empty dictionary on error
        }

        return refs;
    }

    public bool UpdateSymbolicRef(string refName, string targetRef)
    {
        if (string.IsNullOrWhiteSpace(refName) || string.IsNullOrWhiteSpace(targetRef))
            return false;

        try
        {
            var refPath = GetRefPath(refName);
            var refDir = Path.GetDirectoryName(refPath);
            
            if (refDir != null)
            {
                Directory.CreateDirectory(refDir);
            }

            File.WriteAllText(refPath, $"ref: {targetRef}\n");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? ResolveRef(string refName)
    {
        if (string.IsNullOrWhiteSpace(refName))
            return null;

        var visited = new HashSet<string>();
        var currentRef = refName;

        // Follow symbolic references up to a reasonable depth
        while (visited.Count < 10)
        {
            if (visited.Contains(currentRef))
            {
                // Circular reference detected
                return null;
            }

            visited.Add(currentRef);

            var content = ReadRef(currentRef);
            if (content == null)
                return null;

            // If it's a symbolic ref, follow it
            if (content.StartsWith("refs/"))
            {
                currentRef = content;
            }
            else if (IsValidHash(content))
            {
                // Found a hash
                return content;
            }
            else
            {
                return null;
            }
        }

        // Too many levels of indirection
        return null;
    }

    public bool RefExists(string refName)
    {
        if (string.IsNullOrWhiteSpace(refName))
            return false;

        var refPath = GetRefPath(refName);
        return File.Exists(refPath);
    }

    private string GetRefPath(string refName)
    {
        // Handle both formats: "HEAD" and "refs/heads/main"
        if (refName == "HEAD" || refName.StartsWith("refs/"))
        {
            return Path.Combine(_gitDir, refName.Replace('/', Path.DirectorySeparatorChar));
        }
        
        // Assume it's a short branch name like "main"
        return Path.Combine(_gitDir, "refs", "heads", refName);
    }

    private static bool IsValidHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        // SHA-1 hash is 40 hexadecimal characters
        return hash.Length == 40 && Regex.IsMatch(hash, "^[0-9a-f]{40}$", RegexOptions.IgnoreCase);
    }

    private void CleanEmptyDirectories(string directory)
    {
        try
        {
            // Don't clean the refs directory itself
            if (directory == Path.Combine(_gitDir, "refs"))
                return;

            // Only clean if within the refs directory
            if (!directory.StartsWith(Path.Combine(_gitDir, "refs")))
                return;

            if (Directory.GetFiles(directory).Length == 0 && 
                Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory);

                // Recursively clean parent if it's now empty
                var parent = Path.GetDirectoryName(directory);
                if (parent != null)
                {
                    CleanEmptyDirectories(parent);
                }
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }
}
