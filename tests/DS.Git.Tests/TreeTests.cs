using DS.Git.Core;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Tests for tree object operations.
/// </summary>
public class TreeTests : GitTestFixture
{
    [Fact]
    public void WriteTree_ValidEntries_ReturnsHashAndCreatesFile()
    {
        // Arrange
        InitializeRepository();

        var entries = new List<TreeEntry>
        {
            new TreeEntry("100644", "blob", "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391", "file1.txt"),
            new TreeEntry("100644", "blob", "ce013625030ba8dba906f756967f9e9ca394464a", "file2.txt"),
            new TreeEntry("040000", "tree", "d8329fc1cc938780ffdd9f94e0d364e0ea74f579", "subdir")
        };

        // Act
        var hash = Repository.WriteTree(entries);

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(40, hash.Length);

        // Check if file exists
        var objectsDir = Path.Combine(TempDirectory, ".git", "objects");
        var subDir = hash.Substring(0, 2);
        var fileName = hash.Substring(2);
        var objectPath = Path.Combine(objectsDir, subDir, fileName);
        Assert.True(File.Exists(objectPath));
    }

    [Fact]
    public void ReadTree_ValidHash_ReturnsEntries()
    {
        // Arrange
        InitializeRepository();

        var originalEntries = new List<TreeEntry>
        {
            new TreeEntry("100644", "blob", "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391", "file1.txt"),
            new TreeEntry("100644", "blob", "ce013625030ba8dba906f756967f9e9ca394464a", "file2.txt")
        };

        var hash = Repository.WriteTree(originalEntries);
        Assert.NotNull(hash);

        // Act
        var readEntries = Repository.ReadTree(hash);

        // Assert
        Assert.NotNull(readEntries);
        var entriesList = readEntries.ToList();
        Assert.Equal(2, entriesList.Count);
        Assert.Equal("file1.txt", entriesList[0].Name);
        Assert.Equal("file2.txt", entriesList[1].Name);
        Assert.Equal("100644", entriesList[0].Mode);
        Assert.Equal("blob", entriesList[0].Type);
    }

    [Fact]
    public void WriteTree_EmptyEntries_ReturnsValidHash()
    {
        // Arrange
        InitializeRepository();

        // Act
        var hash = Repository.WriteTree(new List<TreeEntry>());

        // Assert
        Assert.NotNull(hash);
        Assert.Equal(40, hash!.Length);
    }

    [Fact]
    public void WriteTree_NullEntries_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var hash = Repository.WriteTree(null);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void ReadTree_InvalidHash_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var result = Repository.ReadTree("invalidhash");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void WriteAndReadTree_RoundTripsData()
    {
        // Arrange
        InitializeRepository();

        var originalEntries = new List<TreeEntry>
        {
            new TreeEntry("100644", "blob", "abc123def456abc123def456abc123def456abc1", "test.txt"),
            new TreeEntry("040000", "tree", "def456abc123def456abc123def456abc123def4", "dir")
        };

        // Act
        var hash = Repository.WriteTree(originalEntries);
        Assert.NotNull(hash);

        var readEntries = Repository.ReadTree(hash);

        // Assert
        Assert.NotNull(readEntries);
        var entriesList = readEntries.ToList();
        Assert.Equal(2, entriesList.Count);

        // Verify first entry (sorted by name, dir comes first)
        Assert.Equal("040000", entriesList[0].Mode);
        Assert.Equal("tree", entriesList[0].Type);
        Assert.Equal("def456abc123def456abc123def456abc123def4", entriesList[0].Hash);
        Assert.Equal("dir", entriesList[0].Name);

        // Verify second entry
        Assert.Equal("100644", entriesList[1].Mode);
        Assert.Equal("blob", entriesList[1].Type);
        Assert.Equal("abc123def456abc123def456abc123def456abc1", entriesList[1].Hash);
        Assert.Equal("test.txt", entriesList[1].Name);
    }
}