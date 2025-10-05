# Tag Implementation Documentation

## Overview
Tag objects in Git are used to mark specific points in a repository's history, typically for releases. This implementation supports both **lightweight tags** (simple references) and **annotated tags** (full Git objects with metadata).

## Architecture

### Core Components

#### 1. ITag Interface (`src/DS.Git.Core/Abstractions/ITag.cs`)
```csharp
public interface ITag
{
    string? Write(TagData? tag);
    TagData? Read(string hash);
}
```

#### 2. TagData Class
```csharp
public class TagData
{
    public string Object { get; set; }     // Hash of tagged object
    public string Type { get; set; }       // Type: commit, tree, blob, or tag
    public string Tag { get; set; }        // Tag name
    public AuthorInfo? Tagger { get; set; } // Tagger information (optional)
    public string Message { get; set; }    // Tag message
}
```

#### 3. Tag Implementation (`src/DS.Git.Core/Tag.cs`)
- Implements ITag interface
- Handles Git tag object format
- SHA-1 hashing and DEFLATE compression
- Storage in `.git/objects/<first-2>/<remaining-38>`

### Git Tag Object Format

```
tag <size>\0object <object-hash>
type <object-type>
tag <tag-name>
tagger <name> <email> <timestamp> <timezone>

<tag message>
```

## Tag Types

### Lightweight Tags
- Simply a reference file pointing to an object
- Stored in `.git/refs/tags/<tagname>`
- Contains only the object hash
- No tag object created in `.git/objects`
- Fast and simple

### Annotated Tags
- Full Git object with metadata
- Contains tagger info, timestamp, and message
- Stored as an object in `.git/objects`
- Reference in `.git/refs/tags/<tagname>` points to tag object
- Tag object points to the actual commit/tree/blob
- Recommended for releases

## CLI Command

### TagCommand (`src/DS.Git.Cli/Commands/TagCommand.cs`)

#### Create Lightweight Tag
```bash
dsgit tag <tagname> [<object>]
```
- Creates a reference to an object (default: HEAD)
- No tag object created
- Fast operation

Example:
```bash
dsgit tag v1.0.0
```

#### Create Annotated Tag
```bash
dsgit tag -a <tagname> -m "<message>" [<object>]
```
- Creates a tag object with metadata
- Requires a message
- Includes tagger information

Example:
```bash
dsgit tag -a v1.0.0 -m "Release version 1.0.0"
```

#### List Tags
```bash
dsgit tag -l
# or simply
dsgit tag
```
- Lists all tags in alphabetical order
- Shows tag names only

### Command Options
- `-a, --annotate`: Create an annotated tag
- `-m, --message <msg>`: Tag message (required for annotated tags)
- `-l, --list`: List all tags
- `<tagname>`: Name of the tag
- `[<object>]`: Object to tag (default: HEAD commit)

## Features

### ✅ Implemented
1. **Annotated Tags**
   - Full Git object with metadata
   - Tagger information (name, email, timestamp, timezone)
   - Multi-line tag messages
   - SHA-1 hash and DEFLATE compression

2. **Lightweight Tags**
   - Simple reference files
   - Point directly to commits/trees/blobs
   - No overhead

3. **Tag Listing**
   - List all tags in repository
   - Alphabetically sorted
   - Fast enumeration

4. **Object Type Detection**
   - Automatically detects object type (commit, tree, blob, tag)
   - Validates object exists before tagging
   - Supports tagging any Git object type

5. **Tag Storage**
   - Proper Git-compatible format
   - Stored in `.git/objects` for annotated tags
   - References in `.git/refs/tags`

6. **Multi-line Messages**
   - Supports multi-line tag messages
   - Preserves formatting with empty lines
   - Full message round-trip

## Usage Examples

### Basic Tagging Workflow

#### 1. Create a Commit
```bash
export GIT_AUTHOR_NAME="John Doe"
export GIT_AUTHOR_EMAIL="john@example.com"

dsgit init
echo "Hello, World!" > hello.txt
dsgit commit -m "Initial commit"
```

#### 2. Create a Lightweight Tag
```bash
dsgit tag v0.1.0
```

This creates `.git/refs/tags/v0.1.0` containing the commit hash.

#### 3. Create an Annotated Tag
```bash
dsgit tag -a v1.0.0 -m "Release version 1.0.0

This release includes:
- Initial implementation
- Core features"
```

This creates:
- Tag object in `.git/objects/<hash>`
- Reference in `.git/refs/tags/v1.0.0`

#### 4. List All Tags
```bash
dsgit tag
# or
dsgit tag -l
```

Output:
```
v0.1.0
v1.0.0
```

### Tag a Specific Object

#### Tag a Tree
```bash
# Get tree hash
TREE_HASH=$(dsgit write-tree)

# Tag the tree
dsgit tag -a snapshot-2024 -m "Snapshot of project" $TREE_HASH
```

#### Tag a Blob
```bash
# Get blob hash
BLOB_HASH=$(dsgit hash-object important-file.txt)

# Tag the blob
dsgit tag important-file $BLOB_HASH
```

## Test Coverage

### TagTests (9 tests)
- ✅ WriteTag_ValidData_ReturnsHashAndCreatesFile
- ✅ ReadTag_ValidHash_ReturnsData
- ✅ WriteAndReadTag_RoundTripsData
- ✅ WriteTag_InvalidData_ReturnsNull
- ✅ ReadTag_InvalidHash_ReturnsNull
- ✅ WriteTag_WithoutTagger_CreatesTag
- ✅ WriteTag_NullData_ReturnsNull
- ✅ WriteTag_MultilineMessage_PreservesFormatting
- ✅ WriteTag_DifferentObjectTypes_AllSupported

### TagCommandTests (6 tests)
- ✅ Execute_AnnotatedTag_CreatesTagObject
- ✅ Execute_LightweightTag_CreatesReference
- ✅ Execute_ListTags_DisplaysAllTags
- ✅ Execute_NoCommits_ReturnsError
- ✅ Execute_AnnotatedTagWithoutMessage_ReturnsError
- ✅ Execute_UninitializedRepository_ReturnsError

**Total: 15 tests, all passing**

## Technical Details

### Tag Object Storage

#### Directory Structure
```
.git/
├── objects/
│   └── ab/
│       └── cdef123...  # Tag object (if annotated)
└── refs/
    └── tags/
        ├── v1.0.0      # Contains hash
        ├── v1.1.0
        └── snapshot
```

#### Tag Object Format (Annotated)
```
tag 123\0object abc123def456abc123def456abc123def456abc1
type commit
tag v1.0.0
tagger John Doe <john@example.com> 1234567890 +0000

Release version 1.0.0
```

#### Lightweight Tag Format
File `.git/refs/tags/v1.0.0`:
```
abc123def456abc123def456abc123def456abc123
```

### Implementation Patterns

#### 1. Git Compatibility
- Follows Git tag object specification
- Compatible with standard Git tools
- Proper SHA-1 hashing
- DEFLATE compression

#### 2. Error Handling
- Validates all inputs
- Proper exception handling
- User-friendly error messages
- Logging integration

#### 3. Code Quality
- Interface-based design (ITag)
- Nullable reference types
- XML documentation
- Comprehensive tests

## Comparison: Lightweight vs Annotated Tags

| Feature | Lightweight | Annotated |
|---------|------------|-----------|
| Git Object | No | Yes |
| Tagger Info | No | Yes |
| Message | No | Yes |
| Timestamp | No | Yes |
| Storage | Reference only | Object + Reference |
| Use Case | Quick marking | Releases, milestones |
| Command | `tag <name>` | `tag -a <name> -m "msg"` |

## Integration with Other Components

### Repository
- `WriteTag(TagData)`: Creates tag object
- `ReadTag(hash)`: Retrieves tag data

### CommandDispatcher
- Routes `tag` command to TagCommand
- Integrated in help output

### File System
- Reads/writes `.git/refs/tags/*`
- Stores objects in `.git/objects`
- Creates directories as needed

## Future Enhancements

### Potential Features
1. **Tag Verification**: GPG signature support
2. **Tag Deletion**: Remove tags
3. **Tag Dereferencing**: Follow tag chains
4. **Pattern Matching**: Filter tags by pattern
5. **Sorting Options**: By date, version, etc.
6. **Detailed Tag Info**: Show full tag object details
7. **Tag Pushing**: Remote tag operations
8. **Tag Descriptions**: Extended descriptions

## Best Practices

### When to Use Lightweight Tags
- Temporary markers
- Local use only
- Quick references
- Personal workflow

### When to Use Annotated Tags
- Release versions
- Milestone markers
- Public tags
- Need metadata
- Audit trail required

### Tag Naming Conventions
- **Releases**: `v1.0.0`, `v2.1.3`
- **Snapshots**: `snapshot-2024-10-05`
- **Features**: `feature-auth-v1`
- **Milestones**: `milestone-1`, `beta-release`

## Troubleshooting

### Common Issues

#### 1. "No commits found"
**Problem**: Trying to tag with no commits in repository.
**Solution**: Create at least one commit first.

#### 2. "Annotated tags require a message"
**Problem**: Using `-a` without `-m`.
**Solution**: Add `-m "message"` or remove `-a` for lightweight tag.

#### 3. "Object not found"
**Problem**: Provided object hash doesn't exist.
**Solution**: Verify the hash with `cat-file` or use HEAD.

## Conclusion

The Tag implementation provides full Git-compatible tagging functionality with both lightweight and annotated tags. It follows the established patterns from Blob, Tree, and Commit implementations, maintaining consistency and quality across the codebase.

**Status**: ✅ Complete and Production-Ready
