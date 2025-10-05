using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to create and manage tags.
/// </summary>
public class TagCommand : ICommand
{
    private readonly ILogger<TagCommand>? _logger;

    public string Name => "tag";
    public string Description => "Create, list, or verify a tag object";

    public TagCommand() { }

    public TagCommand(ILogger<TagCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        try
        {
            // Parse arguments
            string? tagName = null;
            string? objectHash = null;
            string? message = null;
            bool annotated = false;
            bool list = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-a":
                    case "--annotate":
                        annotated = true;
                        break;
                    case "-m":
                    case "--message":
                        if (i + 1 < args.Length)
                        {
                            message = args[i + 1];
                            i++; // Skip the message in next iteration
                        }
                        else
                        {
                            Console.WriteLine("Error: -m requires a message");
                            return 1;
                        }
                        break;
                    case "-l":
                    case "--list":
                        list = true;
                        break;
                    default:
                        if (args[i].StartsWith("-"))
                        {
                            Console.WriteLine($"Unknown option: {args[i]}");
                            Console.WriteLine("Usage: dsgit tag [-a] [-m <message>] <tagname> [<object>]");
                            Console.WriteLine("       dsgit tag -l");
                            return 1;
                        }
                        else if (tagName == null)
                        {
                            tagName = args[i];
                        }
                        else if (objectHash == null)
                        {
                            objectHash = args[i];
                        }
                        break;
                }
            }

            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var repo = new Repository();
            repo.Init(repoPath);

            // List tags
            if (list || (tagName == null && args.Length == 0))
            {
                return ListTags(repoPath);
            }

            // Validate tag name
            if (string.IsNullOrWhiteSpace(tagName))
            {
                Console.WriteLine("Error: Tag name is required");
                Console.WriteLine("Usage: dsgit tag [-a] [-m <message>] <tagname> [<object>]");
                return 1;
            }

            // Get object to tag (default to HEAD)
            if (string.IsNullOrWhiteSpace(objectHash))
            {
                objectHash = GetHeadCommit(repoPath);
                if (string.IsNullOrWhiteSpace(objectHash))
                {
                    Console.WriteLine("Error: No commits found, cannot create tag");
                    return 1;
                }
            }

            // Determine object type
            var objectType = GetObjectType(repo, objectHash);
            if (objectType == null)
            {
                Console.WriteLine($"Error: Object {objectHash} not found");
                return 1;
            }

            // Create annotated tag
            if (annotated)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("Error: Annotated tags require a message (use -m)");
                    return 1;
                }

                // Create tagger info
                var tagger = CreateTaggerInfo();

                var tagData = new TagData(objectHash, objectType, tagName, tagger, message);
                var tagHash = repo.WriteTag(tagData);

                if (tagHash == null)
                {
                    Console.WriteLine("Error: Failed to create tag");
                    return 1;
                }

                // Create tag reference
                CreateTagRef(repoPath, tagName, tagHash);

                Console.WriteLine($"Created annotated tag '{tagName}' -> {objectHash[..7]}");
                _logger?.LogInformation("Created annotated tag {TagName} with hash {Hash}", tagName, tagHash);
            }
            else
            {
                // Create lightweight tag (just a reference)
                CreateTagRef(repoPath, tagName, objectHash);
                Console.WriteLine($"Created lightweight tag '{tagName}' -> {objectHash[..7]}");
                _logger?.LogInformation("Created lightweight tag {TagName} -> {Hash}", tagName, objectHash);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to create tag");
            return 1;
        }
    }

    private static int ListTags(string repoPath)
    {
        var tagsDir = Path.Combine(repoPath, ".git", "refs", "tags");
        if (!Directory.Exists(tagsDir))
        {
            // No tags yet
            return 0;
        }

        var tagFiles = Directory.GetFiles(tagsDir, "*", SearchOption.AllDirectories);
        foreach (var tagFile in tagFiles.OrderBy(f => f))
        {
            var tagName = Path.GetRelativePath(tagsDir, tagFile);
            Console.WriteLine(tagName);
        }

        return 0;
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

    private static string? GetObjectType(Repository repo, string hash)
    {
        // Try to read as different object types
        try
        {
            var commit = repo.ReadCommit(hash);
            if (commit != null) return "commit";
        }
        catch { }

        try
        {
            var tree = repo.ReadTree(hash);
            if (tree != null) return "tree";
        }
        catch { }

        try
        {
            var blob = repo.ReadBlob(hash);
            if (blob != null) return "blob";
        }
        catch { }

        try
        {
            var tag = repo.ReadTag(hash);
            if (tag != null) return "tag";
        }
        catch { }

        return null;
    }

    private static void CreateTagRef(string repoPath, string tagName, string hash)
    {
        var tagsDir = Path.Combine(repoPath, ".git", "refs", "tags");
        Directory.CreateDirectory(tagsDir);

        var tagPath = Path.Combine(tagsDir, tagName);
        File.WriteAllText(tagPath, hash);
    }

    private static AuthorInfo CreateTaggerInfo()
    {
        // TODO: Get from git config
        var name = Environment.GetEnvironmentVariable("GIT_AUTHOR_NAME") ?? "Unknown";
        var email = Environment.GetEnvironmentVariable("GIT_AUTHOR_EMAIL") ?? "unknown@example.com";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timezone = "+0000"; // UTC for now

        return new AuthorInfo(name, email, timestamp, timezone);
    }
}
