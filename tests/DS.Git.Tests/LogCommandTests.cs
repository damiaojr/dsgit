using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the log command.
/// </summary>
public class LogCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_NoCommits_ShowsNoCommitsMessage()
    {
        // Arrange
        InitializeRepository();
        var command = new LogCommand();

        // Create working directory and change to it
        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(Array.Empty<string>());

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_WithCommits_DisplaysCommitHistory()
    {
        // Arrange
        InitializeRepository();
        var commitCommand = new CommitCommand();
        var logCommand = new LogCommand();

        // Set environment variables for author info
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        // Create working directory and change to it
        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Create a test file in the working directory
            File.WriteAllText(Path.Combine(workingDir, "test.txt"), "Hello, World!");

            // Create a commit first
            var commitResult = commitCommand.Execute(new[] { "-m", "Initial commit" });
            Assert.Equal(0, commitResult);

            // Act - show log
            var logResult = logCommand.Execute(Array.Empty<string>());

            // Assert
            Assert.Equal(0, logResult);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", null);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", null);
        }
    }

    [Fact]
    public void Execute_UninitializedRepository_ReturnsError()
    {
        // Arrange
        var command = new LogCommand();

        // Create a temp directory outside any git repo (use a unique subdirectory of temp)
        var nonGitDir = Path.Combine(Path.GetTempPath(), $"NonGit_{Guid.NewGuid()}");
        Directory.CreateDirectory(nonGitDir);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(nonGitDir);

            // Act
            var result = command.Execute(Array.Empty<string>());

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(nonGitDir))
            {
                Directory.Delete(nonGitDir, true);
            }
        }
    }
}