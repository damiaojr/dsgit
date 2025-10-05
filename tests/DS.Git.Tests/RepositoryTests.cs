using DS.Git.Core;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Tests for repository initialization and management.
/// </summary>
public class RepositoryTests : GitTestFixture
{
    [Fact]
    public void Init_ValidPath_ReturnsTrue()
    {
        // Arrange
        var repository = new Repository();

        // Act
        var result = repository.Init("some/path");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Init_NullOrEmptyPath_ReturnsFalse()
    {
        // Arrange
        var repository = new Repository();

        // Act
        var result = repository.Init(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Init_WhitespacePath_ReturnsFalse()
    {
        // Arrange
        var repository = new Repository();

        // Act
        var result = repository.Init("   ");

        // Assert
        Assert.False(result);
    }
}
