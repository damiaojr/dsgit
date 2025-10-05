using DS.Git.Core;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to list the contents of a tree object.
/// </summary>
public class LsTreeCommand : ICommand
{
    private readonly ILogger<LsTreeCommand>? _logger;

    public string Name => "ls-tree";
    public string Description => "List the contents of a tree object";

    public LsTreeCommand() { }

    public LsTreeCommand(ILogger<LsTreeCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dsgit ls-tree <tree-hash>");
            return 1;
        }

        var treeHash = args[0];

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

            var entries = repo.ReadTree(treeHash);

            if (entries == null || !entries.Any())
            {
                Console.WriteLine("Tree is empty or not found");
                return 1;
            }

            foreach (var entry in entries)
            {
                Console.WriteLine($"{entry.Mode} {entry.Type} {entry.Hash}\t{entry.Name}");
            }

            _logger?.LogInformation("Listed tree {Hash} with {Count} entries", treeHash, entries.Count());
            return 0;
        }
        catch (ObjectNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogWarning("Tree not found: {Hash}", treeHash);
            return 1;
        }
        catch (GitException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to read tree {Hash}", treeHash);
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            _logger?.LogError(ex, "Unexpected error reading tree {Hash}", treeHash);
            return 1;
        }
    }
}
