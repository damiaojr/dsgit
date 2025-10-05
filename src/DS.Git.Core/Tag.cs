using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Core;

/// <summary>
/// Represents a Git tag object with read/write operations.
/// </summary>
public class Tag : ITag
{
    private readonly string _repoPath;
    private readonly ILogger<Tag>? _logger;

    public Tag(string repoPath) : this(repoPath, null) { }

    public Tag(string repoPath, ILogger<Tag>? logger)
    {
        _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
        _logger = logger;
    }

    public string? Write(TagData? tag)
    {
        if (tag == null ||
            string.IsNullOrWhiteSpace(tag.Object) ||
            string.IsNullOrWhiteSpace(tag.Type) ||
            string.IsNullOrWhiteSpace(tag.Tag) ||
            string.IsNullOrWhiteSpace(tag.Message))
        {
            _logger?.LogWarning("Invalid tag data provided");
            return null;
        }

        try
        {
            _logger?.LogDebug("Writing tag {TagName} for object {Object}", tag.Tag, tag.Object);

            // Build tag content
            using var contentStream = new MemoryStream();

            // Write object line
            WriteLine(contentStream, $"object {tag.Object}");

            // Write type line
            WriteLine(contentStream, $"type {tag.Type}");

            // Write tag line
            WriteLine(contentStream, $"tag {tag.Tag}");

            // Write tagger line (optional)
            if (tag.Tagger != null)
            {
                WriteLine(contentStream, $"tagger {tag.Tagger.ToGitFormat()}");
            }

            // Write empty line
            WriteLine(contentStream, "");

            // Write tag message
            var messageBytes = Encoding.UTF8.GetBytes(tag.Message);
            contentStream.Write(messageBytes, 0, messageBytes.Length);

            var content = contentStream.ToArray();

            // Create tag header
            string header = $"tag {content.Length}\0";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            // Combine header and content
            byte[] tagData = new byte[headerBytes.Length + content.Length];
            Buffer.BlockCopy(headerBytes, 0, tagData, 0, headerBytes.Length);
            Buffer.BlockCopy(content, 0, tagData, headerBytes.Length, content.Length);

            // Compute SHA-1 hash
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(tagData);
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
                _logger?.LogDebug("Tag {Hash} already exists, skipping write", hash);
                return hash;
            }

            // Create subdirectory if it doesn't exist
            Directory.CreateDirectory(objectDir);

            // Compress and write to file
            using var fileStream = File.Create(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
            deflateStream.Write(tagData, 0, tagData.Length);

            _logger?.LogInformation("Successfully wrote tag {Hash} for {TagName}", hash, tag.Tag);
            return hash;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write tag");
            throw new GitException("Failed to write tag", ex);
        }
    }

    public TagData? Read(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 40)
        {
            _logger?.LogWarning("Invalid hash format: {Hash}", hash);
            return null;
        }

        try
        {
            _logger?.LogDebug("Reading tag {Hash}", hash);

            string objectsDir = Path.Combine(_repoPath, ".git", "objects");
            string subDir = hash[..2];
            string fileName = hash[2..];
            string objectPath = Path.Combine(objectsDir, subDir, fileName);

            if (!File.Exists(objectPath))
            {
                _logger?.LogWarning("Tag {Hash} not found at {Path}", hash, objectPath);
                throw new ObjectNotFoundException(hash);
            }

            // Decompress
            using var fileStream = File.OpenRead(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            deflateStream.CopyTo(memoryStream);
            byte[] tagData = memoryStream.ToArray();

            // Parse header
            int nullIndex = Array.IndexOf(tagData, (byte)0);

            if (nullIndex == -1)
            {
                _logger?.LogError("Invalid tag format: no null terminator found");
                throw new GitException("Invalid tag format: no null terminator");
            }

            // Verify header starts with "tag "
            string header = Encoding.UTF8.GetString(tagData, 0, nullIndex);
            if (!header.StartsWith("tag "))
            {
                _logger?.LogError("Invalid tag header: {Header}", header);
                throw new GitException($"Invalid tag header: {header}");
            }

            // Parse tag content
            string content = Encoding.UTF8.GetString(tagData, nullIndex + 1, tagData.Length - nullIndex - 1);
            var lines = content.Split('\n');

            var tag = new TagData();
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
                    case "object":
                        tag.Object = value;
                        break;
                    case "type":
                        tag.Type = value;
                        break;
                    case "tag":
                        tag.Tag = value;
                        break;
                    case "tagger":
                        tag.Tagger = ParseAuthorInfo(value);
                        break;
                }
            }

            tag.Message = string.Join('\n', messageLines);

            _logger?.LogInformation("Successfully read tag {Hash}", hash);
            return tag;
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
            _logger?.LogError(ex, "Failed to read tag {Hash}", hash);
            throw new GitException($"Failed to read tag {hash}", ex);
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
