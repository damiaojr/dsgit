using DS.Git.Core;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace DS.Git.Tests;

/// <summary>
/// Tests for blob object operations.
/// </summary>
public class BlobTests : GitTestFixture
{
    [Fact]
    public void WriteBlob_ValidContent_ReturnsHashAndCreatesFile()
    {
        // Arrange
        InitializeRepository();
        byte[] content = Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var hash = Repository.WriteBlob(content);

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
    public void ReadBlob_ValidHash_ReturnsContent()
    {
        // Arrange
        InitializeRepository();
        byte[] originalContent = Encoding.UTF8.GetBytes("Hello, World!");
        var hash = Repository.WriteBlob(originalContent);
        Assert.NotNull(hash);

        // Act
        var readContent = Repository.ReadBlob(hash);

        // Assert
        Assert.NotNull(readContent);
        Assert.Equal(originalContent, readContent);
    }

    [Fact]
    public void ReadBlob_InvalidHash_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var readContent = Repository.ReadBlob("invalidhash");

        // Assert
        Assert.Null(readContent);
    }

    [Fact]
    public void ReadBlob_NonExistentHash_ThrowsException()
    {
        // Arrange
        InitializeRepository();

        // Act & Assert
        Assert.Throws<ObjectNotFoundException>(() =>
            Repository.ReadBlob("1234567890123456789012345678901234567890"));
    }

    [Fact]
    public void WriteBlob_NullContent_ReturnsNull()
    {
        // Arrange
        InitializeRepository();

        // Act
        var hash = Repository.WriteBlob(null);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void WriteBlob_UninitializedRepo_ReturnsNull()
    {
        // Arrange
        var uninitializedRepo = new Repository();
        byte[] content = Encoding.UTF8.GetBytes("test");

        // Act
        var hash = uninitializedRepo.WriteBlob(content);

        // Assert
        Assert.Null(hash);
    }

    [Fact]
    public void WriteAndReadBlob_RoundTripsContent()
    {
        // Arrange
        InitializeRepository();

        var testFile = CreateTestFile("test.txt", "roundtrip");
        var fileContent = File.ReadAllBytes(testFile);

        // Act
        var oid = Repository.WriteBlob(fileContent);
        Assert.NotNull(oid);
        var readContent = Repository.ReadBlob(oid);

        // Assert
        Assert.NotNull(readContent);
        Assert.Equal(fileContent, readContent);
        Assert.Equal("roundtrip", Encoding.UTF8.GetString(readContent));
    }

    [Fact]
    public void WriteBlob_ReturnsCorrectHash()
    {
        // Arrange
        InitializeRepository();
        byte[] content = Encoding.UTF8.GetBytes("Hello, World!");
        string header = $"blob {content.Length}\0";
        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
        byte[] blobData = new byte[headerBytes.Length + content.Length];
        Buffer.BlockCopy(headerBytes, 0, blobData, 0, headerBytes.Length);
        Buffer.BlockCopy(content, 0, blobData, headerBytes.Length, content.Length);

        // Compute expected hash
        using var sha1 = SHA1.Create();
        byte[] expectedHashBytes = sha1.ComputeHash(blobData);
        string expectedHash = BitConverter.ToString(expectedHashBytes).Replace("-", "").ToLower();

        // Act
        var actualHash = Repository.WriteBlob(content);

        // Assert
        Assert.Equal(expectedHash, actualHash);
    }
}