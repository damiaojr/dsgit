using DS.Git.Cli;
using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for command dispatcher functionality.
/// </summary>
public class CommandDispatcherTests
{
    private readonly CommandDispatcher _dispatcher;

    public CommandDispatcherTests()
    {
        _dispatcher = new CommandDispatcher();
    }

    [Fact]
    public void Dispatch_InitCommand_ReturnsSuccess()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"DS.Git.Test_{Guid.NewGuid()}");

        // Act
        var result = _dispatcher.Dispatch(new[] { "init", tempDir });

        // Assert
        Assert.Equal(0, result);
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git")));

        // Cleanup
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Dispatch_UnknownCommand_ReturnsError()
    {
        // Act
        var result = _dispatcher.Dispatch(new[] { "unknown" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Dispatch_NoArguments_ShowsUsage()
    {
        // Act
        var result = _dispatcher.Dispatch(Array.Empty<string>());

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Dispatch_InitCommand_InvalidPath_ReturnsError()
    {
        // Act
        var result = _dispatcher.Dispatch(new[] { "init", "" });

        // Assert
        Assert.Equal(1, result);
    }
}