namespace DS.Git.Core.Exceptions;

/// <summary>
/// Base exception for all DS.Git exceptions.
/// </summary>
public class GitException : Exception
{
    public GitException() { }
    public GitException(string message) : base(message) { }
    public GitException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a repository operation fails.
/// </summary>
public class RepositoryException : GitException
{
    public RepositoryException() { }
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a blob operation fails.
/// </summary>
public class BlobException : GitException
{
    public BlobException() { }
    public BlobException(string message) : base(message) { }
    public BlobException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an object is not found in the repository.
/// </summary>
public class ObjectNotFoundException : GitException
{
    public string ObjectHash { get; }

    public ObjectNotFoundException(string objectHash) 
        : base($"Object not found: {objectHash}")
    {
        ObjectHash = objectHash;
    }
}
