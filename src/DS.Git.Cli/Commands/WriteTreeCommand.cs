using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to create a tree object from the working directory.
/// </summary>
public class WriteTreeCommand : ICommand
{
    private readonly ILogger<WriteTreeCommand>? _logger;

    public string Name => "write-tree";
    public string Description => "Create a tree object from the current index/directory";

    public WriteTreeCommand() { }

    public WriteTreeCommand(ILogger<WriteTreeCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        try
        {
            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var repo = new Repository();
            repo.Init(repoPath); // Set the repo path

            // Get all files in the working directory (excluding .git)
            var entries = new List<TreeEntry>();
            var workingDir = repoPath;

            foreach (var file in Directory.GetFiles(workingDir))
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith(".git")) continue;

                // Hash the file content
                var content = File.ReadAllBytes(file);
                var blob = new Blob(repoPath);
                var hash = blob.Write(content);

                if (hash != null)
                {
                    entries.Add(new TreeEntry("100644", "blob", hash, fileName));
                    _logger?.LogDebug("Added file {File} with hash {Hash}", fileName, hash);
                }
            }

            // Add subdirectories (simplified - not recursive for now)
            foreach (var dir in Directory.GetDirectories(workingDir))
            {
                var dirName = Path.GetFileName(dir);
                if (dirName == ".git") continue;

                // For now, create an empty tree for subdirectories
                var subEntries = new List<TreeEntry>();
                foreach (var file in Directory.GetFiles(dir))
                {
                    var fileName = Path.GetFileName(file);
                    var content = File.ReadAllBytes(file);
                    var blob = new Blob(repoPath);
                    var hash = blob.Write(content);

                    if (hash != null)
                    {
                        subEntries.Add(new TreeEntry("100644", "blob", hash, fileName));
                    }
                }

                if (subEntries.Any())
                {
                    var subTreeHash = repo.WriteTree(subEntries);
                    if (subTreeHash != null)
                    {
                        entries.Add(new TreeEntry("040000", "tree", subTreeHash, dirName));
                        _logger?.LogDebug("Added directory {Dir} with hash {Hash}", dirName, subTreeHash);
                    }
                }
            }

            if (!entries.Any())
            {
                // Create an empty tree
            }

            var treeHash = repo.WriteTree(entries);
            if (treeHash != null)
            {
                Console.WriteLine(treeHash);
                _logger?.LogInformation("Created tree {Hash} with {Count} entries", treeHash, entries.Count);
                return 0;
            }
            else
            {
                Console.WriteLine("Failed to write tree");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to write tree");
            return 1;
        }
    }
}
