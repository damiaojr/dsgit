using DS.Git.Core;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to hash a file and write it as a blob object.
/// </summary>
public class HashObjectCommand : ICommand
{
    private readonly ILogger<HashObjectCommand>? _logger;

    public string Name => "hash-object";
    public string Description => "Compute object ID and optionally creates a blob from a file";

    public HashObjectCommand() { }

    public HashObjectCommand(ILogger<HashObjectCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dsgit hash-object <file>");
            return 1;
        }

        var file = args[0];
        
        if (!File.Exists(file))
        {
            Console.WriteLine($"Error: File not found: {file}");
            return 1;
        }

        try
        {
            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var content = File.ReadAllBytes(file);
            var blob = new Blob(repoPath);
            var hash = blob.Write(content);

            if (hash != null)
            {
                Console.WriteLine(hash);
                _logger?.LogInformation("Hashed file {File} to {Hash}", file, hash);
                return 0;
            }
            else
            {
                Console.WriteLine("Failed to write blob");
                return 1;
            }
        }
        catch (BlobException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to hash object from {File}", file);
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            _logger?.LogError(ex, "Unexpected error hashing object from {File}", file);
            return 1;
        }
    }
}
