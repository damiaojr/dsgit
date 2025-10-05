using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DS.Git.Core.Abstractions;
using DS.Git.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DS.Git.Core;

/// <summary>
/// Represents a Git tree object with read/write operations.
/// </summary>
public class Tree : ITree
{
    private readonly string _repoPath;
    private readonly ILogger<Tree>? _logger;

    public Tree(string repoPath) : this(repoPath, null) { }

    public Tree(string repoPath, ILogger<Tree>? logger)
    {
        _repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
        _logger = logger;
    }

    public string? Write(IEnumerable<TreeEntry>? entries)
    {
        if (entries == null)
        {
            _logger?.LogWarning("Attempted to write tree with null entries");
            return null;
        }

        try
        {
            _logger?.LogDebug("Writing tree with {Count} entries", entries.Count());

            // Sort entries by name (Git requirement)
            var sortedEntries = entries.OrderBy(e => e.Name).ToList();

            // Build tree content
            using var contentStream = new MemoryStream();
            
            foreach (var entry in sortedEntries)
            {
                // Validate entry
                if (string.IsNullOrWhiteSpace(entry.Mode) ||
                    string.IsNullOrWhiteSpace(entry.Hash) ||
                    string.IsNullOrWhiteSpace(entry.Name))
                {
                    _logger?.LogWarning("Invalid tree entry: {Name}", entry.Name);
                    continue;
                }

                // Write mode and name: "mode name\0"
                var modeAndName = $"{entry.Mode} {entry.Name}\0";
                var modeAndNameBytes = Encoding.UTF8.GetBytes(modeAndName);
                contentStream.Write(modeAndNameBytes, 0, modeAndNameBytes.Length);

                // Write hash as binary (20 bytes)
                var entryHashBytes = ConvertHexToBytes(entry.Hash);
                contentStream.Write(entryHashBytes, 0, entryHashBytes.Length);
            }

            var content = contentStream.ToArray();

            // Create tree header
            string header = $"tree {content.Length}\0";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            // Combine header and content
            byte[] treeData = new byte[headerBytes.Length + content.Length];
            Buffer.BlockCopy(headerBytes, 0, treeData, 0, headerBytes.Length);
            Buffer.BlockCopy(content, 0, treeData, headerBytes.Length, content.Length);

            // Compute SHA-1 hash
            using var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(treeData);
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
                _logger?.LogDebug("Tree {Hash} already exists, skipping write", hash);
                return hash;
            }

            // Create subdirectory if it doesn't exist
            Directory.CreateDirectory(objectDir);

            // Compress and write to file
            using var fileStream = File.Create(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress);
            deflateStream.Write(treeData, 0, treeData.Length);

            _logger?.LogInformation("Successfully wrote tree {Hash}", hash);
            return hash;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to write tree");
            throw new GitException("Failed to write tree", ex);
        }
    }

    public IEnumerable<TreeEntry>? Read(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || hash.Length != 40)
        {
            _logger?.LogWarning("Invalid hash format: {Hash}", hash);
            return null;
        }

        try
        {
            _logger?.LogDebug("Reading tree {Hash}", hash);

            string objectsDir = Path.Combine(_repoPath, ".git", "objects");
            string subDir = hash[..2];
            string fileName = hash[2..];
            string objectPath = Path.Combine(objectsDir, subDir, fileName);

            if (!File.Exists(objectPath))
            {
                _logger?.LogWarning("Tree {Hash} not found at {Path}", hash, objectPath);
                throw new ObjectNotFoundException(hash);
            }

            // Decompress
            using var fileStream = File.OpenRead(objectPath);
            using var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress);
            using var memoryStream = new MemoryStream();
            deflateStream.CopyTo(memoryStream);
            byte[] treeData = memoryStream.ToArray();

            // Parse header
            int nullIndex = Array.IndexOf(treeData, (byte)0);
            
            if (nullIndex == -1)
            {
                _logger?.LogError("Invalid tree format: no null terminator found");
                throw new GitException("Invalid tree format: no null terminator");
            }

            // Verify header starts with "tree "
            string header = Encoding.UTF8.GetString(treeData, 0, nullIndex);
            if (!header.StartsWith("tree "))
            {
                _logger?.LogError("Invalid tree header: {Header}", header);
                throw new GitException($"Invalid tree header: {header}");
            }

            // Parse entries
            var entries = new List<TreeEntry>();
            int position = nullIndex + 1;

            while (position < treeData.Length)
            {
                // Find next null terminator (end of mode and name)
                int nextNull = -1;
                for (int i = position; i < treeData.Length; i++)
                {
                    if (treeData[i] == 0)
                    {
                        nextNull = i;
                        break;
                    }
                }

                if (nextNull == -1) break;

                // Parse mode and name
                string modeAndName = Encoding.UTF8.GetString(treeData, position, nextNull - position);
                var parts = modeAndName.Split(' ', 2);
                
                if (parts.Length != 2)
                {
                    _logger?.LogWarning("Invalid tree entry format: {Entry}", modeAndName);
                    break;
                }

                string mode = parts[0];
                string name = parts[1];

                // Read hash (20 bytes)
                position = nextNull + 1;
                if (position + 20 > treeData.Length) break;

                byte[] hashBytes = new byte[20];
                Buffer.BlockCopy(treeData, position, hashBytes, 0, 20);
                string entryHash = ConvertBytesToHex(hashBytes);

                // Determine type based on mode
                string type = mode == "040000" ? "tree" : "blob";

                entries.Add(new TreeEntry(mode, type, entryHash, name));

                position += 20;
            }

            _logger?.LogInformation("Successfully read tree {Hash}, {Count} entries", hash, entries.Count);
            return entries;
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
            _logger?.LogError(ex, "Failed to read tree {Hash}", hash);
            throw new GitException($"Failed to read tree {hash}", ex);
        }
    }

    private static byte[] ConvertHexToBytes(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    private static string ConvertBytesToHex(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
