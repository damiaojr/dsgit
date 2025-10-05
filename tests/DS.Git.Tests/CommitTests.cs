using DS.Git.Core;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Tests for commit object operations.
/// </summary>
public class CommitTests : GitTestFixture
{
    [Fact]
    public void WriteCommit_ValidData_ReturnsHashAndCreatesFile()
    {
        // Arrange
        InitializeRepository();

        var author = new AuthorInfo("Test Author", "author@example.com", 1234567890, "+0000");
        var committer = new AuthorInfo("Test Committer", "committer@example.com", 1234567890, "+0000");
        var commitData = new CommitData(
            tree: "abc123def456abc123def456abc123def456abc1",
            parents: new List<string>(),
            author: author,
            committer: committer,
            message: "Initial commit"
        );

        // Act
        var hash = Repository.WriteCommit(commitData);

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(40, hash.Length); // SHA-1 is 40 chars

        // Check if file exists
        var objectsDir = Path.Combine(TempDirectory, ".git", "objects");
        var subDir = hash.Substring(0, 2);
        var fileName = hash.Substring(2);
        var objectPath = Path.Combine(objectsDir, subDir, fileName);
        Assert.True(File.Exists(objectPath));
    }

    [Fact]
    public void ReadCommit_ValidHash_ReturnsData()
    {
        // Arrange
        InitializeRepository();

        var author = new AuthorInfo("Test Author", "author@example.com", 1234567890, "+0000");
        var committer = new AuthorInfo("Test Committer", "committer@example.com", 1234567890, "+0000");
        var originalCommit = new CommitData(
            tree: "abc123def456abc123def456abc123def456abc1",
            parents: new List<string>(),
            author: author,
            committer: committer,
            message: "Initial commit"
        );

        var hash = Repository.WriteCommit(originalCommit);
        Assert.NotNull(hash);

        // Act
        var readCommit = Repository.ReadCommit(hash);

        // Assert
        Assert.NotNull(readCommit);
        Assert.Equal(originalCommit.Tree, readCommit.Tree);
        Assert.Equal(originalCommit.Parents.Count, readCommit.Parents.Count);
        Assert.Equal(originalCommit.Author.Name, readCommit.Author.Name);
        Assert.Equal(originalCommit.Author.Email, readCommit.Author.Email);
        Assert.Equal(originalCommit.Committer.Name, readCommit.Committer.Name);
        Assert.Equal(originalCommit.Committer.Email, readCommit.Committer.Email);
        Assert.Equal(originalCommit.Message, readCommit.Message);
    }

    [Fact]
    public void WriteCommit_WithParents_ReturnsHash()
    {
        // Arrange
        InitializeRepository();

        var author = new AuthorInfo("Test Author", "author@example.com", 1234567890, "+0000");
        var committer = author;
        var commitData = new CommitData(
            tree: "abc123def456abc123def456abc123def456abc1",
            parents: new List<string> { "def456abc123def456abc123def456abc123def4" },
            author: author,
            committer: committer,
            message: "Second commit"
        );

        // Act
        var hash = Repository.WriteCommit(commitData);

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(40, hash.Length);
    }

    [Fact]
    public void ReadCommit_WithParents_ReturnsCorrectData()
    {
        // Arrange
        InitializeRepository();

        var parentHash = "def456abc123def456abc123def456abc123def4";
        var author = new AuthorInfo("Test Author", "author@example.com", 1234567890, "+0000");
        var committer = author;
        var commitData = new CommitData(
            tree: "abc123def456abc123def456abc123def456abc1",
            parents: new List<string> { parentHash },
            author: author,
            committer: committer,
            message: "Second commit"
        );

        var hash = Repository.WriteCommit(commitData);
        Assert.NotNull(hash);

        // Act
        var readCommit = Repository.ReadCommit(hash);

        // Assert
        Assert.NotNull(readCommit);
        Assert.Single(readCommit.Parents);
        Assert.Equal(parentHash, readCommit.Parents[0]);
    }

    [Fact]
    public void WriteCommit_NullData_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var hash = Repository.WriteCommit(null);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void WriteCommit_InvalidData_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        var commitData = new CommitData(); // Empty/invalid data

        // Act
        var hash = Repository.WriteCommit(commitData);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void ReadCommit_InvalidHash_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var commit = Repository.ReadCommit("invalidhash");

        // Assert
        Assert.Null(commit);
    }

    [Fact]
    public void WriteAndReadCommit_RoundTripsData()
    {
        // Arrange
        InitializeRepository();

        var author = new AuthorInfo("John Doe", "john@example.com", 1609459200, "-0500");
        var committer = new AuthorInfo("Jane Smith", "jane@example.com", 1609459260, "-0500");
        var originalCommit = new CommitData(
            tree: "abc123def456abc123def456abc123def456abc1",
            parents: new List<string> { "parent1hash", "parent2hash" },
            author: author,
            committer: committer,
            message: "Fix bug in authentication\n\n- Fixed login issue\n- Added error handling"
        );

        // Act
        var hash = Repository.WriteCommit(originalCommit);
        Assert.NotNull(hash);
        var readCommit = Repository.ReadCommit(hash);

        // Assert
        Assert.NotNull(readCommit);
        Assert.Equal(originalCommit.Tree, readCommit.Tree);
        Assert.Equal(2, readCommit.Parents.Count);
        Assert.Equal("parent1hash", readCommit.Parents[0]);
        Assert.Equal("parent2hash", readCommit.Parents[1]);
        Assert.Equal(originalCommit.Author.Name, readCommit.Author.Name);
        Assert.Equal(originalCommit.Author.Email, readCommit.Author.Email);
        Assert.Equal(originalCommit.Author.Timestamp, readCommit.Author.Timestamp);
        Assert.Equal(originalCommit.Author.Timezone, readCommit.Author.Timezone);
        Assert.Equal(originalCommit.Committer.Name, readCommit.Committer.Name);
        Assert.Equal(originalCommit.Committer.Email, readCommit.Committer.Email);
        Assert.Equal(originalCommit.Committer.Timestamp, readCommit.Committer.Timestamp);
        Assert.Equal(originalCommit.Committer.Timezone, readCommit.Committer.Timezone);
        Assert.Equal(originalCommit.Message, readCommit.Message);
    }
}