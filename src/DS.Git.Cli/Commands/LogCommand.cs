using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to display commit history.
/// </summary>
public class LogCommand : ICommand
{
    private readonly ILogger<LogCommand>? _logger;

    public string Name => "log";
    public string Description => "Show commit history";

    public LogCommand() { }

    public LogCommand(ILogger<LogCommand> logger)
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
            repo.Init(repoPath);

            // Get HEAD commit
            var currentCommitHash = GetHeadCommit(repoPath);
            if (string.IsNullOrWhiteSpace(currentCommitHash))
            {
                Console.WriteLine("No commits yet");
                return 0;
            }

            // Display commit history
            var commitCount = 0;
            var maxCommits = 10; // Limit for now

            while (!string.IsNullOrWhiteSpace(currentCommitHash) && commitCount < maxCommits)
            {
                var commit = repo.ReadCommit(currentCommitHash);
                if (commit == null) break;

                DisplayCommit(currentCommitHash, commit);
                commitCount++;

                // Move to parent commit
                currentCommitHash = commit.Parents.FirstOrDefault();
            }

            if (commitCount == maxCommits)
            {
                Console.WriteLine("... (showing first 10 commits)");
            }

            _logger?.LogInformation("Displayed {Count} commits", commitCount);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to display log");
            return 1;
        }
    }

    private static void DisplayCommit(string hash, CommitData commit)
    {
        Console.WriteLine($"commit {hash}");
        Console.WriteLine($"Author: {commit.Author.ToGitFormat()}");
        Console.WriteLine($"Date:   {DateTimeOffset.FromUnixTimeSeconds(commit.Author.Timestamp):yyyy-MM-dd HH:mm:ss zzz}");
        Console.WriteLine();
        Console.WriteLine($"    {commit.Message.Replace("\n", "\n    ")}");
        Console.WriteLine();
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
}