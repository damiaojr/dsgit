using DS.Git.Core;
using DS.Git.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

/// <summary>
/// Command to initialize a new Git repository.
/// </summary>
public class InitCommand : ICommand
{
    private readonly ILogger<InitCommand>? _logger;

    public string Name => "init";
    public string Description => "Initialize a new Git repository";

    public InitCommand() { }

    public InitCommand(ILogger<InitCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dsgit init <path>");
            return 1;
        }

        var path = args[0];
        
        try
        {
            var repo = new Repository();
            var success = repo.Init(path);

            if (success)
            {
                Console.WriteLine($"Initialized empty repository at {path}");
                _logger?.LogInformation("Initialized repository at {Path}", path);
                return 0;
            }
            else
            {
                Console.WriteLine("Failed to initialize repository");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to initialize repository at {Path}", path);
            return 1;
        }
    }
}
