namespace DS.Git.Cli.Commands;

/// <summary>
/// Base interface for all Git commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the command description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the command with the provided arguments.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    int Execute(string[] args);
}
