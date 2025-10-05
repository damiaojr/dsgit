using DS.Git.Core;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Tests for tag object operations.
/// </summary>
public class TagTests : GitTestFixture
{
    [Fact]
    public void WriteTag_ValidData_ReturnsHashAndCreatesFile()
    {
        // Arrange
        InitializeRepository();

        var tagger = new AuthorInfo("Test Tagger", "tagger@example.com", 1234567890, "+0000");
        var tagData = new TagData(
            @object: "abc123def456abc123def456abc123def456abc1",
            type: "commit",
            tag: "v1.0.0",
            tagger: tagger,
            message: "Release version 1.0.0"
        );

        // Act
        var hash = Repository.WriteTag(tagData);

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
    public void ReadTag_ValidHash_ReturnsData()
    {
        // Arrange
        InitializeRepository();

        var tagger = new AuthorInfo("Test Tagger", "tagger@example.com", 1234567890, "+0000");
        var originalTag = new TagData(
            @object: "abc123def456abc123def456abc123def456abc1",
            type: "commit",
            tag: "v1.0.0",
            tagger: tagger,
            message: "Release version 1.0.0"
        );

        var hash = Repository.WriteTag(originalTag);
        Assert.NotNull(hash);

        // Act
        var readTag = Repository.ReadTag(hash!);

        // Assert
        Assert.NotNull(readTag);
        Assert.Equal(originalTag.Object, readTag.Object);
        Assert.Equal(originalTag.Type, readTag.Type);
        Assert.Equal(originalTag.Tag, readTag.Tag);
        Assert.Equal(originalTag.Message, readTag.Message);
        Assert.NotNull(readTag.Tagger);
        Assert.Equal(tagger.Name, readTag.Tagger.Name);
        Assert.Equal(tagger.Email, readTag.Tagger.Email);
        Assert.Equal(tagger.Timestamp, readTag.Tagger.Timestamp);
        Assert.Equal(tagger.Timezone, readTag.Tagger.Timezone);
    }

    [Fact]
    public void WriteAndReadTag_RoundTripsData()
    {
        // Arrange
        InitializeRepository();

        var tagger = new AuthorInfo("Test Tagger", "tagger@example.com", 1234567890, "+0000");
        var originalTag = new TagData(
            @object: "def456abc123def456abc123def456abc123def4",
            type: "tree",
            tag: "snapshot-2024",
            tagger: tagger,
            message: "Snapshot of the tree at 2024"
        );

        // Act
        var hash = Repository.WriteTag(originalTag);
        var readTag = Repository.ReadTag(hash!);

        // Assert
        Assert.NotNull(readTag);
        Assert.Equal(originalTag.Object, readTag.Object);
        Assert.Equal(originalTag.Type, readTag.Type);
        Assert.Equal(originalTag.Tag, readTag.Tag);
        Assert.Equal(originalTag.Message, readTag.Message);
        Assert.NotNull(readTag.Tagger);
        Assert.Equal(originalTag.Tagger!.Name, readTag.Tagger.Name);
        Assert.Equal(originalTag.Tagger.Email, readTag.Tagger.Email);
    }

    [Fact]
    public void WriteTag_InvalidData_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Missing required fields
        var invalidTag = new TagData();

        // Act
        var hash = Repository.WriteTag(invalidTag);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void ReadTag_InvalidHash_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var tag = Repository.ReadTag("invalidhash");

        // Assert
        Assert.Null(tag);
    }

    [Fact]
    public void WriteTag_WithoutTagger_CreatesTag()
    {
        // Arrange
        InitializeRepository();

        var tagData = new TagData(
            @object: "abc123def456abc123def456abc123def456abc1",
            type: "commit",
            tag: "lightweight-tag",
            tagger: null, // No tagger info
            message: "A tag without tagger info"
        );

        // Act
        var hash = Repository.WriteTag(tagData);

        // Assert
        Assert.NotNull(hash);

        // Read back and verify
        var readTag = Repository.ReadTag(hash!);
        Assert.NotNull(readTag);
        Assert.Null(readTag.Tagger);
        Assert.Equal(tagData.Message, readTag.Message);
    }

    [Fact]
    public void WriteTag_NullData_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var hash = Repository.WriteTag(null);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void WriteTag_MultilineMessage_PreservesFormatting()
    {
        // Arrange
        InitializeRepository();

        var tagger = new AuthorInfo("Test Tagger", "tagger@example.com", 1234567890, "+0000");
        var multilineMessage = @"Release version 2.0.0

This release includes:
- New feature A
- Bug fix B

Breaking changes:
- API change C";

        var tagData = new TagData(
            @object: "abc123def456abc123def456abc123def456abc1",
            type: "commit",
            tag: "v2.0.0",
            tagger: tagger,
            message: multilineMessage
        );

        // Act
        var hash = Repository.WriteTag(tagData);
        var readTag = Repository.ReadTag(hash!);

        // Assert
        Assert.NotNull(readTag);
        Assert.Equal(multilineMessage, readTag.Message);
    }

    [Fact]
    public void WriteTag_DifferentObjectTypes_AllSupported()
    {
        // Arrange
        InitializeRepository();

        var tagger = new AuthorInfo("Test Tagger", "tagger@example.com", 1234567890, "+0000");
        var objectTypes = new[] { "commit", "tree", "blob", "tag" };

        foreach (var objectType in objectTypes)
        {
            var tagData = new TagData(
                @object: "abc123def456abc123def456abc123def456abc1",
                type: objectType,
                tag: $"test-{objectType}",
                tagger: tagger,
                message: $"Tag for {objectType}"
            );

            // Act
            var hash = Repository.WriteTag(tagData);
            var readTag = Repository.ReadTag(hash!);

            // Assert
            Assert.NotNull(readTag);
            Assert.Equal(objectType, readTag.Type);
        }
    }
}
