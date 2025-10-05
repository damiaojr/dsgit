# Architecture Documentation

## Overview

DS.Git follows a **layered architecture** with clear separation between business logic, presentation, and infrastructure concerns.

## Layers

### 1. Core Layer (`DS.Git.Core`)

**Responsibility**: Business logic and domain models

**Key Components**:
- `Repository`: Manages Git repository operations
- `Blob`: Handles Git blob object storage
- `IRepository`, `IBlob`: Abstractions for testing
- `Result<T>`: Error handling without exceptions in return values
- Custom exceptions for exceptional cases

**Design Principles**:
- No dependencies on external layers
- Framework-agnostic
- Pure business logic
- Interface-based for testability

**Patterns Used**:
- Repository Pattern
- Factory Pattern (for object creation)
- Strategy Pattern (for compression, hashing)

### 2. CLI Layer (`DS.Git.Cli`)

**Responsibility**: Command-line interface

**Key Components**:
- `CommandDispatcher`: Routes commands to handlers
- `ICommand`: Command interface
- Individual command implementations

**Design Principles**:
- Depends only on Core layer
- Command Pattern for extensibility
- Single Responsibility per command

**Patterns Used**:
- Command Pattern
- Chain of Responsibility (command routing)

### 3. Test Layer (`DS.Git.Tests`)

**Responsibility**: Automated testing

**Key Components**:
- Unit tests for Core logic
- Integration tests for CLI commands
- Test fixtures and helpers

## Design Decisions

### Why Interfaces?

```csharp
public interface IRepository
{
    bool Init(string path);
    string? WriteBlob(byte[]? content);
    byte[]? ReadBlob(string hash);
}
```

**Benefits**:
1. **Testability**: Mock implementations for unit tests
2. **Flexibility**: Swap implementations (e.g., in-memory for testing)
3. **Dependency Inversion**: Depend on abstractions, not concretions
4. **Future-proof**: Easy to add new storage backends

### Why Custom Exceptions?

```csharp
public class BlobException : GitException { }
public class ObjectNotFoundException : GitException { }
```

**Benefits**:
1. **Clear intent**: Specific exception types for specific scenarios
2. **Better error handling**: Catch specific exceptions
3. **Debugging**: Detailed stack traces with context
4. **User experience**: Meaningful error messages

### Why Command Pattern?

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    int Execute(string[] args);
}
```

**Benefits**:
1. **Extensibility**: Add new commands without modifying existing code
2. **Single Responsibility**: Each command is self-contained
3. **Testability**: Test commands in isolation
4. **Maintainability**: Clear structure and organization

### Why Logging Abstractions?

```csharp
public Blob(string repoPath, ILogger<Blob>? logger)
{
    _logger = logger;
}
```

**Benefits**:
1. **Flexibility**: Support any logging provider (Serilog, NLog, etc.)
2. **Optional**: Logging is optional, not required
3. **Structured logging**: Key-value pairs for better analysis
4. **Performance**: Logger checks before expensive operations

## Error Handling Strategy

### For Expected Errors
Use `Result<T>` for operations that commonly fail:
```csharp
public Result<string> WriteBlob(byte[] content)
{
    if (content == null)
        return Result<string>.Failure("Content cannot be null");
    
    // ... operation
    return Result<string>.Success(hash);
}
```

### For Exceptional Cases
Throw custom exceptions:
```csharp
if (!File.Exists(objectPath))
    throw new ObjectNotFoundException(hash);
```

### For Validation
Validate early, fail fast:
```csharp
public Blob(string repoPath)
{
    _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
}
```

## Object Storage Design

### Git Object Format
```
<type> <size>\0<content>
```

Example blob:
```
blob 13\0Hello, World!
```

### Storage Path
```
.git/objects/<first-2-hash-chars>/<remaining-38-chars>
```

Example:
```
.git/objects/55/7db03de997c86a4a028e1ebd3a1ceb225be238
```

### Compression
- Use DEFLATE (zlib) compression
- Store compressed data on disk
- Decompress when reading

### Hashing
- SHA-1 hash of full object (header + content)
- Content-addressable storage
- Automatic deduplication

## Dependency Flow

```
┌─────────────────┐
│   DS.Git.Cli    │  ← User Interface
└────────┬────────┘
         │ depends on
         ▼
┌─────────────────┐
│  DS.Git.Core    │  ← Business Logic
└─────────────────┘

Tests depend on Core (and CLI for integration tests)
```

**Key Rule**: Dependencies flow inward, never outward
- CLI can depend on Core
- Core cannot depend on CLI
- Tests depend on both

## Future Architecture

### Adding WPF GUI

```
┌─────────────────┐
│   DS.Git.Wpf    │  ← WPF Application
└────────┬────────┘
         │
         ├─► ViewModels (MVVM)
         │
         └─► depends on
             │
             ▼
    ┌─────────────────┐
    │  DS.Git.Core    │
    └─────────────────┘
```

### Adding Dependency Injection

```csharp
// Program.cs
var services = new ServiceCollection()
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<IRepository, Repository>()
    .AddSingleton<CommandDispatcher>()
    .BuildServiceProvider();

var dispatcher = services.GetRequiredService<CommandDispatcher>();
```

### Adding Configuration

```csharp
public class GitOptions
{
    public string DefaultEditor { get; set; }
    public bool EnableGitHooks { get; set; }
    public CompressionLevel CompressionLevel { get; set; }
}

// Inject via IOptions<GitOptions>
```

## Performance Considerations

### Current Optimizations
1. **Lazy initialization**: Create objects only when needed
2. **Streaming**: Use streams for large files
3. **Early validation**: Fail fast before expensive operations
4. **Object caching**: Skip writing duplicate blobs

### Future Optimizations
1. **Async I/O**: Use async/await for file operations
2. **Parallel processing**: Process multiple objects concurrently
3. **Pack files**: Compress multiple objects together
4. **Delta compression**: Store diffs instead of full content

## Testing Strategy

### Unit Tests
Test individual components in isolation:
```csharp
[Fact]
public void WriteBlob_ValidContent_ReturnsHash()
{
    var repo = new Repository();
    repo.Init(tempDir);
    var hash = repo.WriteBlob(content);
    Assert.NotNull(hash);
}
```

### Integration Tests
Test complete workflows:
```csharp
[Fact]
public void FullWorkflow_InitHashRead_Success()
{
    // Init repo
    // Hash file
    // Read back
    // Verify content
}
```

### Test Doubles
- **Mocks**: For interfaces (IRepository, IBlob)
- **Stubs**: For returning fixed data
- **Fakes**: In-memory implementations

## Code Style Guidelines

### Naming Conventions
- PascalCase for classes, methods, properties
- camelCase for parameters, local variables
- _camelCase for private fields
- UPPER_CASE for constants

### Documentation
- XML comments for all public APIs
- /// <summary> for classes and methods
- /// <param> for parameters
- /// <returns> for return values

### Organization
- One class per file
- Group related classes in folders
- Keep files under 300 lines
- Extract complex logic to private methods

## Security Considerations

### Current
- Input validation on all public methods
- Safe file path handling
- No code execution from user input

### Future
- SSH key authentication
- TLS for remote operations
- Signed commits
- Permission checks

## Extension Points

To add new features:

1. **New Git Object Type**: Follow `Blob` pattern
2. **New CLI Command**: Implement `ICommand`
3. **New Storage Backend**: Implement `IRepository`
4. **New Compression**: Strategy pattern in `Blob`
5. **New Hash Algorithm**: Strategy pattern (though SHA-1 is Git standard)

## Resources

- [Git Internals](https://git-scm.com/book/en/v2/Git-Internals-Plumbing-and-Porcelain)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
