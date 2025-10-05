# DS.Git - A Git Implementation in C# for Windows

A production-ready Git implementation built with .NET 8, designed for Windows 11 with best practices and future-proof architecture.

## 🏗️ Architecture

This project follows **Clean Architecture** and **SOLID principles** for maintainability and scalability.

### Project Structure

```
DS.Git.sln
├── src/
│   ├── DS.Git.Core/              # Core business logic
│   │   ├── Abstractions/          # Interfaces (IRepository, IBlob, ITree)
│   │   ├── Common/                # Shared types (Result<T>)
│   │   ├── Exceptions/            # Custom exceptions
│   │   ├── Blob.cs                # Blob object implementation
│   │   ├── Tree.cs                # Tree object implementation
│   │   └── Repository.cs          # Repository implementation
│   └── DS.Git.Cli/               # Command-line interface
│       ├── Commands/              # Command implementations
│       │   ├── ICommand.cs
│       │   ├── InitCommand.cs
│       │   ├── HashObjectCommand.cs
│       │   ├── CatFileCommand.cs
│       │   ├── WriteTreeCommand.cs
│       │   └── LsTreeCommand.cs
│       ├── CommandDispatcher.cs   # Command routing
│       └── Program.cs             # Entry point
└── tests/
    └── DS.Git.Tests/             # Unit tests
        └── RepositoryTests.cs
```

## 🎯 Key Features

### ✅ Implemented
- **Repository Initialization**: Create `.git` directory structure
- **Blob Storage**: Write and read file contents as Git blobs
- **Tree Objects**: Store and retrieve directory structures
- **Commit Objects**: Create and read commits with full Git format
- **Tag Objects**: Annotated and lightweight tags
- **Reference Management**: Branches, symbolic refs, ref operations
- **SHA-1 Hashing**: Content-addressable storage
- **Compression**: zlib/DEFLATE compression for objects
- **CLI Commands**: `init`, `hash-object`, `cat-file`, `write-tree`, `ls-tree`, `commit`, `log`, `tag`, `ref`

### 🚧 Coming Soon
- Staging area (index)
- Branching and merging
- Remote operations
- Remote operations
- WPF GUI for Windows

## 🔧 Design Patterns & Best Practices

### 1. **Interface-Based Design**
All core components implement interfaces (`IRepository`, `IBlob`, `ICommand`) for:
- Easy unit testing with mocks
- Dependency injection support
- Future extensibility

### 2. **Command Pattern**
CLI commands are self-contained classes implementing `ICommand`:
- Single Responsibility Principle
- Easy to add new commands
- Testable in isolation

### 3. **Proper Error Handling**
- Custom exception hierarchy (`GitException`, `BlobException`, `ObjectNotFoundException`)
- No swallowed exceptions
- Meaningful error messages
- Structured logging support

### 4. **Logging Infrastructure**
- Uses `Microsoft.Extensions.Logging.Abstractions`
- Ready for integration with any logging provider
- Supports structured logging
- Optional logger injection

### 5. **Result Types**
`Result<T>` pattern for operations that may fail:
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
}
```

### 6. **Modern C# Features**
- **Nullable reference types** enabled for null safety
- **Range operators** (`hash[..2]` instead of `hash.Substring(0, 2)`)
- **Using declarations** for automatic disposal
- **String interpolation** for readable code

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Windows 11 (for future WPF features)
- WSL2 with Ubuntu (for development)

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Usage
```bash
# Initialize a repository
dotnet run --project src/DS.Git.Cli -- init /path/to/repo

# Hash a file and store as blob
dotnet run --project src/DS.Git.Cli -- hash-object file.txt

# View blob content
dotnet run --project src/DS.Git.Cli -- cat-file -p <hash>

# Create a tree from current directory
dotnet run --project src/DS.Git.Cli -- write-tree

# List tree contents
dotnet run --project src/DS.Git.Cli -- ls-tree <tree-hash>

# Create a commit
dotnet run --project src/DS.Git.Cli -- commit -m "Commit message"

# View commit history
dotnet run --project src/DS.Git.Cli -- log

# Create a lightweight tag
dotnet run --project src/DS.Git.Cli -- tag v1.0.0

# Create an annotated tag
dotnet run --project src/DS.Git.Cli -- tag -a v1.0.0 -m "Release version 1.0"

# List all tags
dotnet run --project src/DS.Git.Cli -- tag -l

# Manage references
# Update a branch reference
dotnet run --project src/DS.Git.Cli -- ref update refs/heads/main <hash>

# Read a reference
dotnet run --project src/DS.Git.Cli -- ref read HEAD

# List all branches
dotnet run --project src/DS.Git.Cli -- ref list refs/heads/

# Create symbolic reference (point HEAD to branch)
dotnet run --project src/DS.Git.Cli -- ref symbolic HEAD refs/heads/main

# Resolve reference to final hash
dotnet run --project src/DS.Git.Cli -- ref resolve HEAD

# Delete a branch
dotnet run --project src/DS.Git.Cli -- ref delete refs/heads/old-feature
```

## 🏛️ Architecture Benefits

### Testability
- All business logic is in `DS.Git.Core` with no dependencies on CLI or UI
- Interfaces enable easy mocking
- Pure functions where possible

### Maintainability
- Clear separation of concerns
- Self-documenting code with XML comments
- Consistent error handling patterns

### Extensibility
- Add new commands by implementing `ICommand`
- Add new object types by following `Blob` pattern
- Easy to add WPF GUI layer without touching core logic

### Performance
- Efficient binary operations
- Streaming for large files
- Cached repository paths
- Object deduplication (same content = same hash)

## 📦 Dependencies

### Core
- `Microsoft.Extensions.Logging.Abstractions` (8.0.0) - Logging infrastructure

### CLI
- `Microsoft.Extensions.Logging.Abstractions` (8.0.0) - Logging infrastructure

### Tests
- `xUnit` - Unit testing framework
- `xUnit.runner.visualstudio` - Test runner
- `coverlet.collector` - Code coverage

## 🛣️ Roadmap

### Phase 1: Core Git Operations (Current)
- [x] Repository initialization
- [x] Blob storage (read/write)
- [x] Basic CLI

### Phase 2: Object Model ✅ COMPLETE
- [x] Tree objects
- [x] Commit objects
- [x] Tag objects
- [x] Reference management

### Phase 3: Advanced Features
- [ ] Staging area (index)
- [ ] Diff operations
- [ ] Merge strategies
- [ ] Branch management

### Phase 4: GUI Application
- [ ] WPF application for Windows 11
- [ ] Repository browser
- [ ] Commit history visualization
- [ ] Interactive staging
- [ ] Merge conflict resolution

### Phase 5: Collaboration
- [ ] Remote repository support
- [ ] Push/pull operations
- [ ] SSH authentication
- [ ] GitHub integration

## 🧪 Testing Strategy

- **Unit Tests**: All core logic tested in isolation
- **Integration Tests**: End-to-end CLI command testing
- **Code Coverage**: Aim for >80% coverage
- **TDD Approach**: Write tests first for new features

## 📚 Learning Resources

This project demonstrates:
- Clean Architecture in .NET
- Command Pattern for CLI applications
- Repository Pattern
- Exception handling best practices
- Async/await patterns (future)
- Dependency Injection (future)
- MVVM for WPF (future)

## 🤝 Contributing

This is a learning project, but contributions are welcome! Please follow:
1. SOLID principles
2. XML documentation for public APIs
3. Unit tests for all features
4. Meaningful commit messages

## 📄 License

MIT License - Feel free to use this for learning or as a foundation for your own projects.

## 🙏 Acknowledgments

- Git internals documentation
- .NET community best practices
- Clean Code principles by Robert C. Martin
