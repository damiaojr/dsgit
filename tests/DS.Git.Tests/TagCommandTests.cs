using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the tag command.
/// </summary>
public class TagCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_AnnotatedTag_CreatesTagObject()
    {
        // Arrange
        InitializeRepository();
        
        // Create a commit first
        var commitCommand = new CommitCommand();
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        File.WriteAllText(Path.Combine(workingDir, "test.txt"), "Hello");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);
            commitCommand.Execute(new[] { "-m", "Initial commit" });

            // Create annotated tag
            var tagCommand = new TagCommand();
            var result = tagCommand.Execute(new[] { "-a", "-m", "Version 1.0", "v1.0.0" });

            // Assert
            Assert.Equal(0, result);

            // Verify tag reference exists
            var tagPath = Path.Combine(TempDirectory, ".git", "refs", "tags", "v1.0.0");
            Assert.True(File.Exists(tagPath));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", null);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", null);
        }
    }

    [Fact]
    public void Execute_LightweightTag_CreatesReference()
    {
        // Arrange
        InitializeRepository();
        
        // Create a commit first
        var commitCommand = new CommitCommand();
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        File.WriteAllText(Path.Combine(workingDir, "test.txt"), "Hello");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);
            commitCommand.Execute(new[] { "-m", "Initial commit" });

            // Create lightweight tag (no -a, no -m)
            var tagCommand = new TagCommand();
            var result = tagCommand.Execute(new[] { "v0.1.0" });

            // Assert
            Assert.Equal(0, result);

            // Verify tag reference exists
            var tagPath = Path.Combine(TempDirectory, ".git", "refs", "tags", "v0.1.0");
            Assert.True(File.Exists(tagPath));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", null);
            Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", null);
        }
    }

    [Fact]
    public void Execute_ListTags_DisplaysAllTags()
    {
        // Arrange
        InitializeRepository();
        
        // Create a commit and tags
        var commitCommand = new CommitCommand();
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        File.WriteAllText(Path.Combine(workingDir, "test.txt"), "Hello");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);
            commitCommand.Execute(new[] { "-m", "Initial commit" });

            var tagCommand = new TagCommand();
            tagCommand.Execute(new[] { "v1.0.0" });
            tagCommand.Execute(new[] { "v1.1.0" });

            // Act - list tags
            var result = tagCommand.Execute(new[] { "-l" });

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

    [Fact]
    public void Execute_NoCommits_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var tagCommand = new TagCommand();

        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = tagCommand.Execute(new[] { "v1.0.0" });

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_AnnotatedTagWithoutMessage_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        
        // Create a commit first
        var commitCommand = new CommitCommand();
        Environment.SetEnvironmentVariable("GIT_AUTHOR_NAME", "Test Author");
        Environment.SetEnvironmentVariable("GIT_AUTHOR_EMAIL", "author@example.com");

        var workingDir = Path.Combine(TempDirectory, "working");
        Directory.CreateDirectory(workingDir);
        File.WriteAllText(Path.Combine(workingDir, "test.txt"), "Hello");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);
            commitCommand.Execute(new[] { "-m", "Initial commit" });

            // Act - annotated tag without message
            var tagCommand = new TagCommand();
            var result = tagCommand.Execute(new[] { "-a", "v1.0.0" });

            // Assert
            Assert.Equal(1, result);
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
        var tagCommand = new TagCommand();

        // Create a temp directory outside any git repo
        var nonGitDir = Path.Combine(Path.GetTempPath(), $"NonGit_{Guid.NewGuid()}");
        Directory.CreateDirectory(nonGitDir);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(nonGitDir);

            // Act
            var result = tagCommand.Execute(new[] { "v1.0.0" });

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
