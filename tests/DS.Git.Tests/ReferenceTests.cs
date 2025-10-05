using DS.Git.Core;
using DS.Git.Core.Exceptions;
using Xunit;

namespace DS.Git.Tests;

public class ReferenceTests : GitTestFixture
{
    [Fact]
    public void UpdateRef_ValidData_CreatesReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var refName = "refs/heads/main";

        // Act
        var reference = new Reference(TempDirectory);
        var result = reference.UpdateRef(refName, hash);

        // Assert
        Assert.True(result);
        
        var refPath = Path.Combine(TempDirectory, ".git", "refs", "heads", "main");
        Assert.True(File.Exists(refPath));
        
        var content = File.ReadAllText(refPath).Trim();
        Assert.Equal(hash, content);
    }

    [Fact]
    public void ReadRef_ExistingRef_ReturnsHash()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var refName = "refs/heads/main";

        var reference = new Reference(TempDirectory);
        reference.UpdateRef(refName, hash);

        // Act
        var result = reference.ReadRef(refName);

        // Assert
        Assert.Equal(hash, result);
    }

    [Fact]
    public void ReadRef_NonExistentRef_ReturnsNull()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.ReadRef("refs/heads/nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteRef_ExistingRef_RemovesReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var refName = "refs/heads/feature";

        var reference = new Reference(TempDirectory);
        reference.UpdateRef(refName, hash);

        var refPath = Path.Combine(TempDirectory, ".git", "refs", "heads", "feature");
        Assert.True(File.Exists(refPath));

        // Act
        var result = reference.DeleteRef(refName);

        // Assert
        Assert.True(result);
        Assert.False(File.Exists(refPath));
    }

    [Fact]
    public void DeleteRef_HEAD_ReturnsFalse()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.DeleteRef("HEAD");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ListRefs_MultipleRefs_ReturnsAll()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);
        reference.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");
        reference.UpdateRef("refs/heads/develop", "abcdef1234567890123456789012345678901234");
        reference.UpdateRef("refs/tags/v1.0", "fedcba0987654321098765432109876543210987");

        // Act
        var refs = reference.ListRefs();

        // Assert
        Assert.Equal(3, refs.Count);
        Assert.Contains("refs/heads/main", refs.Keys);
        Assert.Contains("refs/heads/develop", refs.Keys);
        Assert.Contains("refs/tags/v1.0", refs.Keys);
    }

    [Fact]
    public void ListRefs_WithPattern_ReturnsFiltered()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);
        reference.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");
        reference.UpdateRef("refs/heads/develop", "abcdef1234567890123456789012345678901234");
        reference.UpdateRef("refs/tags/v1.0", "fedcba0987654321098765432109876543210987");

        // Act
        var refs = reference.ListRefs("refs/heads/");

        // Assert
        Assert.Equal(2, refs.Count);
        Assert.Contains("refs/heads/main", refs.Keys);
        Assert.Contains("refs/heads/develop", refs.Keys);
        Assert.DoesNotContain("refs/tags/v1.0", refs.Keys);
    }

    [Fact]
    public void UpdateSymbolicRef_ValidData_CreatesSymbolicReference()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.UpdateSymbolicRef("HEAD", "refs/heads/main");

        // Assert
        Assert.True(result);
        
        var headPath = Path.Combine(TempDirectory, ".git", "HEAD");
        var content = File.ReadAllText(headPath).Trim();
        Assert.Equal("ref: refs/heads/main", content);
    }

    [Fact]
    public void ResolveRef_DirectRef_ReturnsHash()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var reference = new Reference(TempDirectory);
        reference.UpdateRef("refs/heads/main", hash);

        // Act
        var result = reference.ResolveRef("refs/heads/main");

        // Assert
        Assert.Equal(hash, result);
    }

    [Fact]
    public void ResolveRef_SymbolicRef_FollowsToHash()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var reference = new Reference(TempDirectory);
        reference.UpdateRef("refs/heads/main", hash);
        reference.UpdateSymbolicRef("HEAD", "refs/heads/main");

        // Act
        var result = reference.ResolveRef("HEAD");

        // Assert
        Assert.Equal(hash, result);
    }

    [Fact]
    public void RefExists_ExistingRef_ReturnsTrue()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);
        reference.UpdateRef("refs/heads/main", "1234567890123456789012345678901234567890");

        // Act
        var result = reference.RefExists("refs/heads/main");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RefExists_NonExistentRef_ReturnsFalse()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.RefExists("refs/heads/nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UpdateRef_InvalidHash_ReturnsFalse()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.UpdateRef("refs/heads/main", "invalid-hash");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Constructor_NonGitDirectory_ThrowsException()
    {
        // Arrange
        var nonGitDir = Path.Combine(TempDirectory, "not-a-repo");
        Directory.CreateDirectory(nonGitDir);

        // Act & Assert
        Assert.Throws<GitException>(() => new Reference(nonGitDir));
    }

    [Fact]
    public void UpdateRef_CreatesIntermediateDirectories()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var refName = "refs/remotes/origin/main";

        var reference = new Reference(TempDirectory);

        // Act
        var result = reference.UpdateRef(refName, hash);

        // Assert
        Assert.True(result);
        
        var refPath = Path.Combine(TempDirectory, ".git", "refs", "remotes", "origin", "main");
        Assert.True(File.Exists(refPath));
    }

    [Fact]
    public void DeleteRef_CleansUpEmptyDirectories()
    {
        // Arrange
        var repo = new Repository();
        repo.Init(TempDirectory);

        var hash = "1234567890123456789012345678901234567890";
        var refName = "refs/remotes/origin/feature";

        var reference = new Reference(TempDirectory);
        reference.UpdateRef(refName, hash);

        var featureDir = Path.Combine(TempDirectory, ".git", "refs", "remotes", "origin");
        Assert.True(Directory.Exists(featureDir));

        // Act
        reference.DeleteRef(refName);

        // Assert
        Assert.False(Directory.Exists(featureDir));
    }
}
