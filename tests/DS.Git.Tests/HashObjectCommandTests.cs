using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the hash-object command.
/// </summary>
public class HashObjectCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_ValidFile_ReturnsHash()
    {
        // Arrange
        InitializeRepository();
        var command = new HashObjectCommand();
        var testFile = CreateTestFile("test.txt", "Hello, World!");

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);
        var workTestFile = Path.Combine(workingDir, "test.txt");
        File.Copy(testFile, workTestFile);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { "test.txt" });

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_WriteOption_CreatesObjectFile()
    {
        // Arrange
        InitializeRepository();
        var command = new HashObjectCommand();
        var testFile = CreateTestFile("test.txt", "Hello, World!");

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);
        var workTestFile = Path.Combine(workingDir, "test.txt");
        File.Copy(testFile, workTestFile);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { "test.txt" });

            // Assert
            Assert.Equal(0, result);

            // Verify object was created
            var objectsDir = Path.Combine(TempDirectory, ".git", "objects");
            Assert.True(Directory.GetFiles(objectsDir, "*", SearchOption.AllDirectories).Length > 0);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_NoArguments_ReturnsError()
    {
        // Arrange
        var command = new HashObjectCommand();

        // Act
        var result = command.Execute(Array.Empty<string>());

        // Assert
        Assert.Equal(1, result);
    }
}