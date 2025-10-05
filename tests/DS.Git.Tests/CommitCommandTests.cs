using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the commit command.
/// </summary>
public class CommitCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_ValidCommit_CreatesCommitAndUpdatesHead()
    {
        // Arrange
        InitializeRepository();
        var command = new CommitCommand();

        // Create a test file
        CreateTestFile("test.txt", "Hello, World!");

        // Set environment variables for author info
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);
        CreateTestFile("work/test.txt", "Hello, World!");

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { "-m", "Initial commit" });

            // Assert
            Assert.Equal(0, result);

            // Check that HEAD was created/updated
            var headPath = Path.Combine(TempDirectory, ".git", "HEAD");
            Assert.True(File.Exists(headPath));

            var headContent = File.ReadAllText(headPath);
            Assert.Equal("ref: refs/heads/master", headContent);

            // Check that refs/heads/master exists
            var masterRefPath = Path.Combine(TempDirectory, ".git", "refs", "heads", "master");
            Assert.True(File.Exists(masterRefPath));

            var commitHash = File.ReadAllText(masterRefPath).Trim();
            Assert.Equal(40, commitHash.Length);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", null);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", null);
        }
    }

    [Fact]
    public void Execute_NoMessage_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var command = new CommitCommand();

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(Array.Empty<string>());

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_UninitializedRepository_ReturnsError()
    {
        // Arrange
        var command = new CommitCommand();

        // Create a temp directory outside any git repo
        var nonGitDir = Path.Combine(Path.GetTempPath(), $"NonGit_{Guid.NewGuid()}");
        Directory.CreateDirectory(nonGitDir);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(nonGitDir);

            // Act
            var result = command.Execute(new[] { "-m", "Test commit" });

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

    [Fact]
    public void Execute_EmptyRepositoryWithAllowEmpty_CreatesCommit()
    {
        // Arrange
        InitializeRepository();
        var command = new CommitCommand();

        // Set environment variables for author info
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { "-m", "Empty commit", "--allow-empty" });

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", null);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", null);
        }
    }
}