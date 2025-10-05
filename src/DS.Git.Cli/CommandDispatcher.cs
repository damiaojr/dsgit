using DS.Git.Cli.Commands;

namespace DS.Git.Cli;

/// <summary>
/// Command dispatcher that routes commands to their handlers.
/// </summary>
public class CommandDispatcher
{
    private readonly Dictionary<string, ICommand> _commands;

    public CommandDispatcher()
    {
        _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { "init", new InitCommand() },
            { "hash-object", new HashObjectCommand() },
            { "cat-file", new CatFileCommand() },
            { "write-tree", new WriteTreeCommand() },
            { "ls-tree", new LsTreeCommand() },
            { "commit", new CommitCommand() },
            { "log", new LogCommand() },
            { "tag", new TagCommand() },
            { "ref", new RefCommand() }
        };
    }

    public int Dispatch(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return 1;
        }

        var commandName = args[0].ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        if (_commands.TryGetValue(commandName, out var command))
        {
            return command.Execute(commandArgs);
        }

        Console.WriteLine($"Error: Unknown command '{commandName}'");
        Console.WriteLine();
        ShowUsage();
        return 1;
    }

    private void ShowUsage()
    {
        Console.WriteLine("DS.Git - A Git implementation in C#");
        Console.WriteLine();
        Console.WriteLine("Usage: dsgit <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Available commands:");
        
        foreach (var cmd in _commands.Values)
        {
            Console.WriteLine($"  {cmd.Name,-15} {cmd.Description}");
        }
    }
}
