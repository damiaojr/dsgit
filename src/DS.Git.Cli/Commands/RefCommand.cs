using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to manage Git references (branches, tags, HEAD).
/// </summary>
public class RefCommand : ICommand
{
    private readonly ILogger<RefCommand>? _logger;

    public string Name => "ref";
    public string Description => "Manage Git references (create, read, delete, list)";

    public RefCommand() { }

    public RefCommand(ILogger<RefCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        try
        {
            // Find repository
            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var repo = new Repository();
            repo.Init(repoPath);

            // Parse arguments
            if (args.Length == 0)
            {
                ShowUsage();
                return 0;
            }

            var command = args[0];

            switch (command)
            {
                case "update":
                    return UpdateRef(repo, args);
                
                case "read":
                    return ReadRef(repo, args);
                
                case "delete":
                    return DeleteRef(repo, args);
                
                case "list":
                    return ListRefs(repo, args);
                
                case "symbolic":
                    return UpdateSymbolicRef(repo, args);
                
                case "resolve":
                    return ResolveRef(repo, args);
                
                case "exists":
                    return CheckRefExists(repo, args);
                
                default:
                    Console.WriteLine($"Error: Unknown subcommand '{command}'");
                    ShowUsage();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to execute ref command");
            return 1;
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Usage: dsgit ref <subcommand> [options]");
        Console.WriteLine();
        Console.WriteLine("Subcommands:");
        Console.WriteLine("  update <ref> <hash>         Create or update a reference");
        Console.WriteLine("  read <ref>                  Read a reference value");
        Console.WriteLine("  delete <ref>                Delete a reference");
        Console.WriteLine("  list [pattern]              List all references (optionally filtered)");
        Console.WriteLine("  symbolic <ref> <target>     Create or update a symbolic reference");
        Console.WriteLine("  resolve <ref>               Resolve a reference to its final hash");
        Console.WriteLine("  exists <ref>                Check if a reference exists");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dsgit ref update refs/heads/main abc123...");
        Console.WriteLine("  dsgit ref read HEAD");
        Console.WriteLine("  dsgit ref delete refs/heads/feature");
        Console.WriteLine("  dsgit ref list refs/heads/");
        Console.WriteLine("  dsgit ref symbolic HEAD refs/heads/main");
        Console.WriteLine("  dsgit ref resolve HEAD");
    }

    private int UpdateRef(IRepository repo, string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Error: update requires <ref> and <hash> arguments");
            return 1;
        }

        var refName = args[1];
        var hash = args[2];

        if (repo.UpdateRef(refName, hash))
        {
            Console.WriteLine($"Updated ref '{refName}' to {hash}");
            _logger?.LogInformation("Updated reference {RefName} to {Hash}", refName, hash);
            return 0;
        }

        Console.WriteLine($"Error: Failed to update ref '{refName}'");
        return 1;
    }

    private int ReadRef(IRepository repo, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: read requires <ref> argument");
            return 1;
        }

        var refName = args[1];
        var value = repo.ReadRef(refName);

        if (value == null)
        {
            Console.WriteLine($"Error: Reference '{refName}' not found");
            return 1;
        }

        Console.WriteLine(value);
        return 0;
    }

    private int DeleteRef(IRepository repo, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: delete requires <ref> argument");
            return 1;
        }

        var refName = args[1];

        if (repo.DeleteRef(refName))
        {
            Console.WriteLine($"Deleted ref '{refName}'");
            _logger?.LogInformation("Deleted reference {RefName}", refName);
            return 0;
        }

        Console.WriteLine($"Error: Failed to delete ref '{refName}'");
        return 1;
    }

    private int ListRefs(IRepository repo, string[] args)
    {
        var pattern = args.Length > 1 ? args[1] : null;
        var refs = repo.ListRefs(pattern);

        if (refs.Count == 0)
        {
            Console.WriteLine("No references found");
            return 0;
        }

        foreach (var kvp in refs.OrderBy(r => r.Key))
        {
            Console.WriteLine($"{kvp.Value} {kvp.Key}");
        }

        return 0;
    }

    private int UpdateSymbolicRef(IRepository repo, string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Error: symbolic requires <ref> and <target> arguments");
            return 1;
        }

        var refName = args[1];
        var targetRef = args[2];

        if (repo.UpdateSymbolicRef(refName, targetRef))
        {
            Console.WriteLine($"Updated symbolic ref '{refName}' to '{targetRef}'");
            _logger?.LogInformation("Updated symbolic reference {RefName} to {Target}", refName, targetRef);
            return 0;
        }

        Console.WriteLine($"Error: Failed to update symbolic ref '{refName}'");
        return 1;
    }

    private int ResolveRef(IRepository repo, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: resolve requires <ref> argument");
            return 1;
        }

        var refName = args[1];
        var hash = repo.ResolveRef(refName);

        if (hash == null)
        {
            Console.WriteLine($"Error: Could not resolve reference '{refName}'");
            return 1;
        }

        Console.WriteLine(hash);
        return 0;
    }

    private int CheckRefExists(IRepository repo, string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: exists requires <ref> argument");
            return 1;
        }

        var refName = args[1];
        var exists = repo.RefExists(refName);

        Console.WriteLine(exists ? "true" : "false");
        return exists ? 0 : 1;
    }
}
