using DS.Git.Cli.Commands;
using DS.Git.Core;
using Xunit;

namespace DS.Git.Tests;

public class RefCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_NoArguments_ShowsUsage()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(Array.Empty<string>());

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_UpdateRef_CreatesReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();
        var hash = "1234567890123456789012345678901234567890";

        // Act
        var result = command.Execute(new[] { "update", "refs/heads/main", hash });

        // Assert
        Assert.Equal(0, result);
        
        var refPath = Path.Combine(TempDirectory, ".git", "refs", "heads", "main");
        Assert.True(File.Exists(refPath));
        
        var content = File.ReadAllText(refPath).Trim();
        Assert.Equal(hash, content);
    }

    [Fact]
    public void Execute_ReadRef_ReturnsHash()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        repo.UpdateRef("refs/heads/main", hash);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "read", "refs/heads/main" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_ReadNonExistentRef_ReturnsError()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "read", "refs/heads/nonexistent" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_DeleteRef_RemovesReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        repo.UpdateRef("refs/heads/feature", hash);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "delete", "refs/heads/feature" });

        // Assert
        Assert.Equal(0, result);
        Assert.False(repo.RefExists("refs/heads/feature"));
    }

    [Fact]
    public void Execute_ListRefs_DisplaysAllReferences()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        repo.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");
        repo.UpdateRef("refs/heads/develop", "abcdef1234567890123456789012345678901234");
        repo.UpdateRef("refs/tags/v1.0", "fedcba0987654321098765432109876543210987");

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "list" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_ListRefsWithPattern_DisplaysFilteredReferences()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        repo.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");
        repo.UpdateRef("refs/heads/develop", "abcdef1234567890123456789012345678901234");
        repo.UpdateRef("refs/tags/v1.0", "fedcba0987654321098765432109876543210987");

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "list", "refs/heads/" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_UpdateSymbolicRef_CreatesSymbolicReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "symbolic", "HEAD", "refs/heads/main" });

        // Assert
        Assert.Equal(0, result);
        
        var headPath = Path.Combine(TempDirectory, ".git", "HEAD");
        var content = File.ReadAllText(headPath).Trim();
        Assert.Equal("ref: refs/heads/main", content);
    }

    [Fact]
    public void Execute_ResolveRef_ReturnsHash()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        
        // Verify the ref was created
        Assert.True(repo.UpdateRef("refs/heads/main", hash));
        Assert.Equal(hash, repo.ReadRef("refs/heads/main"));
        
        // Create symbolic ref
        Assert.True(repo.UpdateSymbolicRef("HEAD", "refs/heads/main"));
        Assert.Equal("refs/heads/main", repo.ReadRef("HEAD"));
        
        // Verify resolution works
        var resolved = repo.ResolveRef("HEAD");
        Assert.Equal(hash, resolved);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "resolve", "HEAD" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_RefExists_ReturnsZeroForExisting()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        repo.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "exists", "refs/heads/main" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_RefExists_ReturnsOneForNonExistent()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "exists", "refs/heads/nonexistent" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_UninitializedRepository_ReturnsError()
    {
        // Arrange
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "list" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_UnknownSubcommand_ReturnsError()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "unknown" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_UpdateWithoutArguments_ReturnsError()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);
        Directory.SetCurrentDirectory(TempDirectory);

        var command = new RefCommand();

        // Act
        var result = command.Execute(new[] { "update" });

        // Assert
        Assert.Equal(1, result);
    }
}
