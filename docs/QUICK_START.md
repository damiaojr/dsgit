# Quick Start Guide

## For Developers New to the Project

### 1. Understanding the Codebase (5 minutes)

#### Project Structure
```
DS.Git/
├── src/
│   ├── DS.Git.Core/     ← Business logic (pure C#)
│   └── DS.Git.Cli/      ← Command-line interface
└── tests/
    └── DS.Git.Tests/    ← Unit tests
```

#### Key Files
1. **`Repository.cs`** - Main Git repository operations
2. **`Blob.cs`** - Git blob (file) storage
3. **`CommandDispatcher.cs`** - Routes CLI commands
4. **`Commands/*.cs`** - Individual command implementations

### 2. Build and Run (2 minutes)

```bash
# Clone and build
git clone <your-repo>
cd DS.Git
dotnet build

# Run tests
dotnet test

# Try it out
dotnet run --project src/DS.Git.Cli
```

### 3. Your First Command (10 minutes)

Let's add a new command to list all objects in the repository.

#### Step 1: Create the command class

Create `src/DS.Git.Cli/Commands/ListObjectsCommand.cs`:

```csharp
using DS.Git.Core;
using Microsoft.Extensions.Logging;

namespace DS.Git.Cli.Commands;

public class ListObjectsCommand : ICommand
{
    private readonly ILogger<ListObjectsCommand>? _logger;

    public string Name => "list-objects";
    public string Description => "List all objects in the repository";

    public ListObjectsCommand() { }

    public ListObjectsCommand(ILogger<ListObjectsCommand> logger)
    {
        _logger = logger;
    }

    public int Execute(string[] args)
    {
        try
        {
            var repoPath = Repository.FindRepoPath(Directory.GetCurrentDirectory());
            if (repoPath == null)
            {
                Console.WriteLine("Error: Not a git repository");
                return 1;
            }

            var objectsDir = Path.Combine(repoPath, ".git", "objects");
            var count = 0;

            foreach (var dir in Directory.GetDirectories(objectsDir))
            {
                var prefix = Path.GetFileName(dir);
                if (prefix.Length != 2) continue;

                foreach (var file in Directory.GetFiles(dir))
                {
                    var suffix = Path.GetFileName(file);
                    var hash = prefix + suffix;
                    Console.WriteLine(hash);
                    count++;
                }
            }

            Console.WriteLine($"\nTotal: {count} objects");
            _logger?.LogInformation("Listed {Count} objects", count);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _logger?.LogError(ex, "Failed to list objects");
            return 1;
        }
    }
}
```

#### Step 2: Register the command

Edit `src/DS.Git.Cli/CommandDispatcher.cs`:

```csharp
public CommandDispatcher()
{
    _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
    {
        { "init", new InitCommand() },
        { "hash-object", new HashObjectCommand() },
        { "cat-file", new CatFileCommand() },
        { "list-objects", new ListObjectsCommand() }  // Add this line
    };
}
```

#### Step 3: Test it

```bash
# Build
dotnet build

# Create a test repo
dotnet run --project src/DS.Git.Cli -- init /tmp/test-repo
cd /tmp/test-repo

# Create some objects
echo "Hello" > file1.txt
dotnet run --project ~/DS.Git/src/DS.Git.Cli -- hash-object file1.txt

# List all objects
dotnet run --project ~/DS.Git/src/DS.Git.Cli -- list-objects
```

**That's it!** You've added a new command following the project's architecture.

---

## Common Tasks

### Adding a New Command
1. Create `Commands/YourCommand.cs` implementing `ICommand`
2. Register in `CommandDispatcher.cs`
3. Write tests in `DS.Git.Tests`

### Adding Core Functionality
1. Add method to `Repository.cs` or create new class
2. Update `IRepository` interface if needed
3. Write unit tests
4. Consider adding a CLI command to expose it

### Running Tests
```bash
# All tests
dotnet test

# Specific test
dotnet test --filter "FullyQualifiedName~WriteBlob"

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Debugging
```bash
# Add breakpoints in VS Code, then:
cd src/DS.Git.Cli
dotnet run -- init /tmp/debug-repo
```

---

## Architecture Cheat Sheet

### When to use each layer:

**DS.Git.Core**
- ✅ Git operations (init, read, write)
- ✅ Object models (blob, tree, commit)
- ✅ Business logic
- ❌ UI code
- ❌ User interaction

**DS.Git.Cli**
- ✅ Command parsing
- ✅ User messages
- ✅ Command-line interface
- ❌ Business logic
- ❌ Direct file operations (use Core)

### Design patterns used:

```csharp
// Repository Pattern
IRepository repo = new Repository();

// Command Pattern
ICommand cmd = new InitCommand();
cmd.Execute(args);

// Strategy Pattern (future)
ICompressionStrategy compression = new DeflateCompression();

// Factory Pattern (future)
IGitObject obj = GitObjectFactory.Create(type, data);
```

---

## Common Mistakes to Avoid

❌ **Don't put business logic in CLI commands**
```csharp
// Bad
public class InitCommand : ICommand
{
    public int Execute(string[] args)
    {
        // Creating .git directories here ❌
    }
}
```

✅ **Do delegate to Core layer**
```csharp
// Good
public class InitCommand : ICommand
{
    public int Execute(string[] args)
    {
        var repo = new Repository();
        repo.Init(path); // Core handles the logic ✅
    }
}
```

---

❌ **Don't swallow exceptions**
```csharp
// Bad
catch (Exception)
{
    return null; // User doesn't know what went wrong ❌
}
```

✅ **Do log and throw meaningful exceptions**
```csharp
// Good
catch (Exception ex)
{
    _logger?.LogError(ex, "Failed to write blob");
    throw new BlobException("Failed to write blob", ex); ✅
}
```

---

❌ **Don't use concrete types in tests**
```csharp
// Bad
var repo = new Repository(); // Hard to mock ❌
```

✅ **Do use interfaces**
```csharp
// Good
IRepository repo = new Repository(); // Can mock in tests ✅
```

---

## Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Test Failures
```bash
# Run specific test with detailed output
dotnet test --filter "FullyQualifiedName~TestName" --logger "console;verbosity=detailed"
```

### Runtime Errors
1. Check the exception message and stack trace
2. Look for custom exceptions (BlobException, etc.)
3. Check logs if logging is enabled
4. Add breakpoints and debug

---

## Best Practices Checklist

When adding new code:
- [ ] Follows existing patterns
- [ ] Has XML documentation
- [ ] Has unit tests
- [ ] Uses interfaces where appropriate
- [ ] Handles errors properly
- [ ] Includes logging statements
- [ ] Validates input early
- [ ] No hardcoded paths
- [ ] No swallowed exceptions
- [ ] Follows SOLID principles

---

## Resources

### Documentation
- `README.md` - Project overview
- `docs/ARCHITECTURE.md` - Detailed architecture
- `docs/REFACTORING_SUMMARY.md` - Recent improvements

### Git Internals
- [Git Book - Internals](https://git-scm.com/book/en/v2/Git-Internals-Plumbing-and-Porcelain)
- [Git Objects](https://git-scm.com/book/en/v2/Git-Internals-Git-Objects)

### .NET Best Practices
- [.NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

## Getting Help

1. **Check the documentation** in `docs/`
2. **Look at existing code** for patterns
3. **Run the tests** to understand expected behavior
4. **Use the debugger** to step through code
5. **Read the Git internals** documentation

---

## Next Steps

Once comfortable with the basics:
1. Read `docs/ARCHITECTURE.md` for deep dive
2. Try implementing tree objects
3. Add more commands
4. Improve test coverage
5. Start planning the WPF GUI

**Happy coding! 🚀**
