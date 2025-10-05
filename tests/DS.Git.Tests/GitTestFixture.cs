using System.IO;
using DS.Git.Core;
using DS.Git.Core.Abstractions;

namespace DS.Git.Tests;

/// <summary>
/// Base test fixture providing common setup for Git tests.
/// </summary>
public abstract class GitTestFixture : IDisposable
{
    protected readonly string TempDirectory;
    protected readonly string OriginalDirectory;
    protected readonly IRepository Repository;

    protected GitTestFixture()
    {
        // Get original directory, but handle case where current directory is invalid
        try
        {
            OriginalDirectory = Directory.GetCurrentDirectory();
        }
        catch
        {
            OriginalDirectory = Path.GetTempPath();
        }
        
        TempDirectory = Path.Combine(Path.GetTempPath(), $"DS.Git.Test_{Guid.NewGuid()}");
        Repository = new Repository();

        // Ensure clean directory
        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, true);
        }

        Directory.CreateDirectory(TempDirectory);
    }

    /// <summary>
    /// Initializes a Git repository in the temp directory.
    /// </summary>
    protected bool InitializeRepository()
    {
        return Repository.Init(TempDirectory);
    }

    /// <summary>
    /// Creates a test file with specified content.
    /// </summary>
    protected string CreateTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(TempDirectory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// Creates a test directory.
    /// </summary>
    protected string CreateTestDirectory(string dirName)
    {
        var dirPath = Path.Combine(TempDirectory, dirName);
        Directory.CreateDirectory(dirPath);
        return dirPath;
    }

    public void Dispose()
    {
        // Restore original directory in case tests changed it
        try
        {
            Directory.SetCurrentDirectory(OriginalDirectory);
        }
        catch
        {
            // Ignore if directory doesn't exist or is invalid
        }

        if (Directory.Exists(TempDirectory))
        {
            try
            {
                Directory.Delete(TempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}