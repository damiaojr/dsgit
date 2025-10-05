using DS.Git.Cli.Commands;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the cat-file command.
/// </summary>
public class CatFileCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_ValidBlobHash_ReturnsContent()
    {
        // Arrange
        InitializeRepository();
        var command = new CatFileCommand();
        var testContent = "Hello, World!";
        var hash = Repository.WriteBlob(System.Text.Encoding.UTF8.GetBytes(testContent));

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { "-p", hash! });

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [Fact]
    public void Execute_InvalidHash_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var command = new CatFileCommand();

        // Act
        var result = command.Execute(new[] { "-p", "invalidhash" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_NonExistentHash_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var command = new CatFileCommand();

        // Act
        var result = command.Execute(new[] { "-p", "1234567890123456789012345678901234567890" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_NoArguments_ReturnsError()
    {
        // Arrange
        var command = new CatFileCommand();

        // Act
        var result = command.Execute(Array.Empty<string>());

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_MissingOption_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var command = new CatFileCommand();
        var testContent = "Hello, World!";
        var hash = Repository.WriteBlob(System.Text.Encoding.UTF8.GetBytes(testContent));

        // Act
        var result = command.Execute(new[] { hash! });

        // Assert
        Assert.Equal(1, result);
    }
}