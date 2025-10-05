using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to create a new commit.
/// </summary>
public class CommitCommand : ICommand
{
    private readonly ILogger<CommitCommand>? _logger;

    public string Name => "commit";
    public string Description => "Create a new commit with the current changes";

    public CommitCommand() { }

    public CommitCommand(ILogger<CommitCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        try
        {
            // Parse arguments
            var message = string.Empty;
            var allowEmpty = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-m":
                    case "--message":
                        if (i + 1 < args.Length)
                        {
                            message = args[i + 1];
                            i++; // Skip the message in next iteration
                        }
                        else
                        {
                            Console.WriteLine("Error: -m requires a commit message");
                            return 1;
                        }
                        break;
                    case "--allow-empty":
                        allowEmpty = true;
                        break;
                    default:
                        Console.WriteLine($"Unknown option: {args[i]}");
                        Console.WriteLine("Usage: dsgit commit [-m <message>] [--allow-empty]");
                        return 1;
                }
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine("Error: Commit message is required (use -m)");
                return 1;
            }

            var currentDir = Directory.GetCurrentDirectory();
            var repoPath = Repository.FindRepoPath(currentDir);
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var repo = new Repository();
            repo.Init(repoPath); // Set the repo path

            // For now, create a simple tree from the current directory
            // TODO: This should use the staging area when implemented
            var entries = CreateTreeEntries(repoPath);
            if (!entries.Any() && !allowEmpty)
            {
                Console.WriteLine("Error: Nothing to commit (use --allow-empty to override)");
                return 1;
            }

            var treeHash = repo.WriteTree(entries);
            if (treeHash == null)
            {
                Console.WriteLine("Error: Failed to create tree");
                return 1;
            }

            // Get parent commit (HEAD)
            var parentHash = GetHeadCommit(repoPath);

            var parents = new List<string>();
            if (!string.IsNullOrWhiteSpace(parentHash))
            {
                parents.Add(parentHash);
            }

            // Create author/committer info
            var author = CreateAuthorInfo();
            var committer = author; // For now, author and committer are the same

            var commitData = new CommitData(treeHash, parents, author, committer, message);
            var commitHash = repo.WriteCommit(commitData);

            if (commitHash == null)
            {
                Console.WriteLine("Error: Failed to create commit");
                return 1;
            }

            // Update HEAD to point to new commit
            UpdateHead(repoPath, commitHash);

            Console.WriteLine($"[{commitHash[..7]}] {message.Split('\n')[0]}");
            if (parents.Any())
            {
                Console.WriteLine($" {parents.Count} file(s) changed, {entries.Count()} insertions(+)");
            }
            else
            {
                Console.WriteLine(" 1 file(s) changed, 1 insertions(+)");
            }

            _logger?.LogInformation("Created commit {Hash} with message: {Message}", commitHash, message);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to create commit");
            return 1;
        }
    }

    private static List<TreeEntry> CreateTreeEntries(string repoPath)
    {
        var entries = new List<TreeEntry>();
        var workingDir = Directory.GetCurrentDirectory();

        foreach (var file in Directory.GetFiles(workingDir))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith(".git")) continue;

            try
            {
                var content = File.ReadAllBytes(file);
                var repo = new Repository();
                repo.Init(repoPath);
                var blob = new Blob(repoPath);
                var hash = blob.Write(content);

                if (hash != null)
                {
                    entries.Add(new TreeEntry("100644", "blob", hash, fileName));
                }
            }
            catch (Exception)
            {
                // Skip files that can't be read
            }
        }

        return entries;
    }

    private static string? GetHeadCommit(string repoPath)
    {
        var headPath = Path.Combine(repoPath, ".git", "HEAD");
        if (!File.Exists(headPath)) return null;

        var headContent = File.ReadAllText(headPath).Trim();
        if (headContent.StartsWith("ref: "))
        {
            var refPath = headContent[5..]; // Remove "ref: "
            var fullRefPath = Path.Combine(repoPath, ".git", refPath);
            if (File.Exists(fullRefPath))
            {
                return File.ReadAllText(fullRefPath).Trim();
            }
        }

        return headContent;
    }

    private static void UpdateHead(string repoPath, string commitHash)
    {
        var headPath = Path.Combine(repoPath, ".git", "HEAD");
        var refsHeadsMaster = Path.Combine(repoPath, ".git", "refs", "heads", "master");

        // Create refs/heads/master if it doesn't exist
        Directory.CreateDirectory(Path.GetDirectoryName(refsHeadsMaster)!);
        File.WriteAllText(refsHeadsMaster, commitHash);

        // Update HEAD to point to refs/heads/master
        File.WriteAllText(headPath, "ref: refs/heads/master");
    }

    private static AuthorInfo CreateAuthorInfo()
    {
        // TODO: Get from git config
        var name = Environment.GetEnvironmentVariable("GIT_AUTHOR_NAME") ?? "Unknown";
        var email = Environment.GetEnvironmentVariable("GIT_AUTHOR_EMAIL") ?? "unknown@example.com";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timezone = "+0000"; // UTC for now

        return new AuthorInfo(name, email, timestamp, timezone);
    }
}