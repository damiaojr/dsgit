using DS.Git.Cli.Commands;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Tests for the ls-tree command.
/// </summary>
public class LsTreeCommandTests : GitTestFixture
{
    [Fact]
    public void Execute_ValidTreeHash_ReturnsEntries()
    {
        // Arrange
        InitializeRepository();
        var command = new LsTreeCommand();

        // Create a tree with some entries
        var entries = new List<TreeEntry>
        {
            new TreeEntry("100644", "blob", "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391", "file1.txt"),
            new TreeEntry("100644", "blob", "ce013625030ba8dba906f756967f9e9ca394464a", "file2.txt")
        };
        var treeHash = Repository.WriteTree(entries);

        // Create a working directory inside the repo
        var workingDir = Path.Combine(TempDirectory, "work");
        Directory.CreateDirectory(workingDir);

        // Change to working directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(workingDir);

            // Act
            var result = command.Execute(new[] { treeHash! });

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
        var command = new LsTreeCommand();

        // Act
        var result = command.Execute(new[] { "invalidhash" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_NonExistentHash_ReturnsError()
    {
        // Arrange
        InitializeRepository();
        var command = new LsTreeCommand();

        // Act
        var result = command.Execute(new[] { "1234567890123456789012345678901234567890" });

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_NoArguments_ReturnsError()
    {
        // Arrange
        var command = new LsTreeCommand();

        // Act
        var result = command.Execute(Array.Empty<string>());

        // Assert
        Assert.Equal(1, result);
    }
}