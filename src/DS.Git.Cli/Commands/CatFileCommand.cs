using System.Text;
using DS.Git.Core;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to display the contents of a Git object.
/// </summary>
public class CatFileCommand : ICommand
{
    private readonly ILogger<CatFileCommand>? _logger;

    public string Name => "cat-file";
    public string Description => "Provide content or type and size information for repository objects";

    public CatFileCommand() { }

    public CatFileCommand(ILogger<CatFileCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        if (args.Length < 2 || args[0] != "-p")
        {
            Console.WriteLine("Usage: dsgit cat-file -p <hash>");
            return 1;
        }

        var objectHash = args[1];

        try
        {
            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository (or any of the parent directories)");
                return 1;
            }

            var blob = new Blob(repoPath);
            var objectContent = blob.Read(objectHash);

            if (objectContent != null)
            {
                Console.WriteLine(Encoding.UTF8.GetString(objectContent));
                _logger?.LogInformation("Retrieved object {Hash}", objectHash);
                return 0;
            }
            else
            {
                Console.WriteLine("Object not found or invalid");
                return 1;
            }
        }
        catch (ObjectNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogWarning("Object not found: {Hash}", objectHash);
            return 1;
        }
        catch (BlobException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to read object {Hash}", objectHash);
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            _logger?.LogError(ex, "Unexpected error reading object {Hash}", objectHash);
            return 1;
        }
    }
}
