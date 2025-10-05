using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Core;

/// <summary>
/// Represents a Git blob object with read/write operations.
/// </summary>
public class Blob : IBlob
{
    private readonly string _repoPath;
    private readonly ILogger<Blob>? _logger;

    public Blob(string repoPath) : this(repoPath, null) { }

    public Blob(string repoPath, ILogger<Blob>? logger)
    {
        _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
        _logger = logger;
    }

    public string? Write(byte[]? content)
    {
        if (content == null)
        {
            _logger?.LogWarning("Attempted to write null content");
            return null;
        }

        try
        {
            _logger?.LogDebug("Writing blob with {Size} bytes", content.Length);

            // Create blob header
            string header = $"blob {content.Length}\0";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            // Combine header and content
            byte[] blobData = new byte[headerBytes.Length + content.Length];
            Buffer.BlockCopy(headerBytes, 0, blobData, 0, headerBytes.Length);
            Buffer.BlockCopy(content, 0, blobData, headerBytes.Length, content.Length);

            // Compute SHA-1 hash
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(blobData);
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
                _logger?.LogDebug("Blob {Hash} already exists, skipping write", hash);
                return hash;
            }

            // Create subdirectory if it doesn't exist
            Directory.CreateDirectory(objectDir);

            // Compress and write to file
            using var fileStream = File.Create(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
            deflateStream.Write(blobData, 0, blobData.Length);

            _logger?.LogInformation("Successfully wrote blob {Hash}", hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write blob");
            throw new BlobException("Failed to write blob", ex);
        }
    }

    public byte[]? Read(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 40)
        {
            _logger?.LogWarning("Invalid hash format: {Hash}", hash);
            return null;
        }

        try
        {
            _logger?.LogDebug("Reading blob {Hash}", hash);

            string objectsDir = Path.Combine(_repoPath, ".git", "objects");
            string subDir = hash[..2];
            string fileName = hash[2..];
            string objectPath = Path.Combine(objectsDir, subDir, fileName);

            if (!File.Exists(objectPath))
            {
                _logger?.LogWarning("Blob {Hash} not found at {Path}", hash, objectPath);
                throw new ObjectNotFoundException(hash);
            }

            // Decompress
            using var fileStream = File.OpenRead(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            deflateStream.CopyTo(memoryStream);
            byte[] blobData = memoryStream.ToArray();

            // Parse header
            int nullIndex = Array.IndexOf(blobData, (byte)0);
            
            if (nullIndex == -1)
            {
                _logger?.LogError("Invalid blob format: no null terminator found");
                throw new BlobException("Invalid blob format: no null terminator");
            }

            // Verify header starts with "blob "
            string header = Encoding.UTF8.GetString(blobData, 0, nullIndex);
            if (!header.StartsWith("blob "))
            {
                _logger?.LogError("Invalid blob header: {Header}", header);
                throw new BlobException($"Invalid blob header: {header}");
            }

            // Extract content
            byte[] content = new byte[blobData.Length - nullIndex - 1];
            Buffer.BlockCopy(blobData, nullIndex + 1, content, 0, content.Length);

            _logger?.LogInformation("Successfully read blob {Hash}, {Size} bytes", hash, content.Length);
            return content;
        }
        catch (ObjectNotFoundException)
        {
            throw;
        }
        catch (BlobException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read blob {Hash}", hash);
            throw new BlobException($"Failed to read blob {hash}", ex);
        }
    }
}