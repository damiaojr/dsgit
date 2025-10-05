using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the write-tree command.
/// </summary>
public class WriteTreeCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_InDirectory_ReturnsHash()
    {
        // Arrange
        InitializeRepository();
        var command = new WriteTreeCommand();

        // Create some test files
        CreateTestFile("file1.txt", "content1");
        CreateTestFile("file2.txt", "content2");
        CreateTestDirectory("subdir");
        CreateTestFile("subdir/file3.txt", "content3");

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
    public void Execute_EmptyDirectory_ReturnsEmptyTreeHash()
    {
        // Arrange
        InitializeRepository();
        var command = new WriteTreeCommand();

        // Create working directory and change to it (repo has no files)
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
    public void Execute_UninitializedRepository_ReturnsError()
    {
        // Arrange
        var command = new WriteTreeCommand();

        // Create a temp directory outside any git repo
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