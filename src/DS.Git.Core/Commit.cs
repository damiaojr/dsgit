using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Core;

/// <summary>
/// Represents a Git commit object with read/write operations.
/// </summary>
public class Commit : ICommit
{
    private readonly string _repoPath;
    private readonly ILogger<Commit>? _logger;

    public Commit(string repoPath) : this(repoPath, null) { }

    public Commit(string repoPath, ILogger<Commit>? logger)
    {
        _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
        _logger = logger;
    }

    public string? Write(CommitData? commit)
    {
        if (commit == null ||
            string.IsNullOrWhiteSpace(commit.Tree) ||
            commit.Author == null ||
            commit.Committer == null ||
            string.IsNullOrWhiteSpace(commit.Message))
        {
            _logger?.LogWarning("Invalid commit data provided");
            return null;
        }

        try
        {
            _logger?.LogDebug("Writing commit for tree {Tree}", commit.Tree);

            // Build commit content
            using var contentStream = new MemoryStream();

            // Write tree line
            WriteLine(contentStream, $"tree {commit.Tree}");

            // Write parent lines
            foreach (var parent in commit.Parents)
            {
                if (!string.IsNullOrWhiteSpace(parent))
                {
                    WriteLine(contentStream, $"parent {parent}");
                }
            }

            // Write author and committer lines
            WriteLine(contentStream, $"author {commit.Author.ToGitFormat()}");
            WriteLine(contentStream, $"committer {commit.Committer.ToGitFormat()}");

            // Write empty line
            WriteLine(contentStream, "");

            // Write commit message
            var messageBytes = Encoding.UTF8.GetBytes(commit.Message);
            contentStream.Write(messageBytes, 0, messageBytes.Length);

            var content = contentStream.ToArray();

            // Create commit header
            string header = $"commit {content.Length}\0";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            // Combine header and content
            byte[] commitData = new byte[headerBytes.Length + content.Length];
            Buffer.BlockCopy(headerBytes, 0, commitData, 0, headerBytes.Length);
            Buffer.BlockCopy(content, 0, commitData, headerBytes.Length, content.Length);

            // Compute SHA-1 hash
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(commitData);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // Get object path
            string objectsDir = Path.Combine(_repoPath, ".git", "objects");
            string subDir = hash[..2];
            string fileName = hash[2..];
            string objectDir = Path.Combine(objectsDir, subDir);
            string objectPath = Path.Combine(objectDir, fileName);

            // Skip if object already exists
            if (File.Exists(objectPath))
            {
                _logger?.LogDebug("Commit {Hash} already exists, skipping write", hash);
                return hash;
            }

            // Create subdirectory if it doesn't exist
            Directory.CreateDirectory(objectDir);

            // Compress and write to file
            using var fileStream = File.Create(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
            deflateStream.Write(commitData, 0, commitData.Length);

            _logger?.LogInformation("Successfully wrote commit {Hash}", hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write commit");
            throw new GitException("Failed to write commit", ex);
        }
    }

    public CommitData? Read(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 40)
        {
            _logger?.LogWarning("Invalid hash format: {Hash}", hash);
            return null;
        }

        try
        {
            _logger?.LogDebug("Reading commit {Hash}", hash);

            string objectsDir = Path.Combine(_repoPath, ".git", "objects");
            string subDir = hash[..2];
            string fileName = hash[2..];
            string objectPath = Path.Combine(objectsDir, subDir, fileName);

            if (!File.Exists(objectPath))
            {
                _logger?.LogWarning("Commit {Hash} not found at {Path}", hash, objectPath);
                throw new ObjectNotFoundException(hash);
            }

            // Decompress
            using var fileStream = File.OpenRead(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            deflateStream.CopyTo(memoryStream);
            byte[] commitData = memoryStream.ToArray();

            // Parse header
            int nullIndex = Array.IndexOf(commitData, (byte)0);

            if (nullIndex == -1)
            {
                _logger?.LogError("Invalid commit format: no null terminator found");
                throw new GitException("Invalid commit format: no null terminator");
            }

            // Verify header starts with "commit "
            string header = Encoding.UTF8.GetString(commitData, 0, nullIndex);
            if (!header.StartsWith("commit "))
            {
                _logger?.LogError("Invalid commit header: {Header}", header);
                throw new GitException($"Invalid commit header: {header}");
            }

            // Parse commit content
            string content = Encoding.UTF8.GetString(commitData, nullIndex + 1, commitData.Length - nullIndex - 1);
            var lines = content.Split('\n');

            var commit = new CommitData();
            var messageLines = new List<string>();
            bool inMessage = false;

            foreach (var line in lines)
            {
                if (inMessage)
                {
                    messageLines.Add(line);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    inMessage = true;
                    continue;
                }

                var parts = line.Split(' ', 2);
                if (parts.Length != 2) continue;

                var key = parts[0];
                var value = parts[1];

                switch (key)
                {
                    case "tree":
                        commit.Tree = value;
                        break;
                    case "parent":
                        commit.Parents.Add(value);
                        break;
                    case "author":
                        commit.Author = ParseAuthorInfo(value);
                        break;
                    case "committer":
                        commit.Committer = ParseAuthorInfo(value);
                        break;
                }
            }

            commit.Message = string.Join('\n', messageLines);

            _logger?.LogInformation("Successfully read commit {Hash}", hash);
            return commit;
        }
        catch (ObjectNotFoundException)
        {
            throw;
        }
        catch (GitException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read commit {Hash}", hash);
            throw new GitException($"Failed to read commit {hash}", ex);
        }
    }

    private static void WriteLine(MemoryStream stream, string line)
    {
        var bytes = Encoding.UTF8.GetBytes(line + "\n");
        stream.Write(bytes, 0, bytes.Length);
    }

    private static AuthorInfo ParseAuthorInfo(string authorLine)
    {
        // Format: "Name <email> timestamp timezone"
        // Find the last space before the timezone (which is +XXXX or -XXXX)
        var parts = authorLine.Trim().Split(' ');
        if (parts.Length < 3) return new AuthorInfo();

        // Timezone is the last part
        var timezone = parts[^1];

        // Timestamp is the second-to-last part
        if (!long.TryParse(parts[^2], out var timestamp))
            return new AuthorInfo();

        // Everything before timestamp is name and email
        var nameAndEmail = string.Join(' ', parts[..^2]);

        // Parse name and email: "Name <email>"
        var emailStart = nameAndEmail.IndexOf('<');
        var emailEnd = nameAndEmail.IndexOf('>');

        if (emailStart == -1 || emailEnd == -1 || emailEnd < emailStart)
            return new AuthorInfo();

        var name = nameAndEmail[..emailStart].Trim();
        var email = nameAndEmail[(emailStart + 1)..emailEnd].Trim();

        return new AuthorInfo(name, email, timestamp, timezone);
    }
}