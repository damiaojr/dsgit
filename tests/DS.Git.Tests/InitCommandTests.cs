using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the init command.
/// </summary>
public class InitCommandTests
{
    [Fact]
    public void Execute_ValidPath_CreatesGitDirectory()
    {
        // Arrange
        var command = new InitCommand();
        var tempDir = Path.Combine(Path.GetTempPath(), $"DS.Git.Test_{Guid.NewGuid()}");

        // Act
        var result = command.Execute(new[] { tempDir });

        // Assert
        Assert.Equal(0, result);
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git")));
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git", "objects")));
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git", "refs")));
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git", "refs", "heads")));

        // Cleanup
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Execute_NoArguments_ReturnsError()
    {
        // Arrange
        var command = new InitCommand();

        // Act
        var result = command.Execute(Array.Empty<string>());

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_InvalidPath_ReturnsError()
    {
        // Arrange
        var command = new InitCommand();

        // Act
        var result = command.Execute(new[] { "" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_ExistingDirectory_CreatesGitStructure()
    {
        // Arrange
        var command = new InitCommand();
        var tempDir = Path.Combine(Path.GetTempPath(), $"DS.Git.Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        // Act
        var result = command.Execute(new[] { tempDir });

        // Assert
        Assert.Equal(0, result);
        Assert.True(Directory.Exists(Path.Combine(tempDir, ".git")));

        // Cleanup
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }
}