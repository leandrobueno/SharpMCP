using System.IO.Compression;
using System.Text;
using DotNet.Globbing;
using SharpMCP.Core.Schema;
using SharpMCP.Core.Tools;
using JsonRequiredAttribute = SharpMCP.Core.Schema.JsonRequiredAttribute;

namespace SharpMCP.Tools.Common.FileSystem;

#region Archive Tool Arguments

/// <summary>
/// Arguments for archive operations.
/// </summary>
public class ArchiveOperationArgs
{
    /// <summary>
    /// Type of archive operation: 'extract', 'create', 'list', 'test', 'info'.
    /// </summary>
    [JsonRequired]
    [JsonDescription("Type of archive operation: 'extract', 'create', 'list', 'test', 'info'")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Path to existing archive file (for extract/list/test/info operations).
    /// </summary>
    [JsonDescription("Path to existing archive file (for extract/list/test/info operations)")]
    public string? ArchivePath { get; set; }

    /// <summary>
    /// Source path for files/directory to compress (for create operation).
    /// </summary>
    [JsonDescription("Source path for files/directory to compress (for create operation)")]
    public string? SourcePath { get; set; }

    /// <summary>
    /// Path where new archive should be created (for create operation).
    /// </summary>
    [JsonDescription("Path where new archive should be created (for create operation)")]
    public string? ArchiveOutputPath { get; set; }

    /// <summary>
    /// Directory where files should be extracted (for extract operation).
    /// </summary>
    [JsonDescription("Directory where files should be extracted (for extract operation)")]
    public string? ExtractToPath { get; set; }

    /// <summary>
    /// Archive operation options.
    /// </summary>
    [JsonDescription("Archive operation options")]
    public ArchiveOperationOptions? Options { get; set; }
}

/// <summary>
/// Archive operation options.
/// </summary>
public class ArchiveOperationOptions
{
    /// <summary>
    /// Overwrite existing files during extraction (default: false).
    /// </summary>
    [JsonDescription("Overwrite existing files during extraction (default: false)")]
    public bool Overwrite { get; set; } = false;

    /// <summary>
    /// Preserve file permissions during extraction (default: true).
    /// </summary>
    [JsonDescription("Preserve file permissions during extraction (default: true)")]
    public bool PreservePermissions { get; set; } = true;

    /// <summary>
    /// Compression level 0-9 for creating archives (default: 6).
    /// </summary>
    [JsonDescription("Compression level 0-9 for creating archives (default: 6)")]
    public int CompressionLevel { get; set; } = 6;

    /// <summary>
    /// Preview operations without executing (default: false).
    /// </summary>
    [JsonDescription("Preview operations without executing (default: false)")]
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Maximum extraction size in bytes (default: 1GB).
    /// </summary>
    [JsonDescription("Maximum extraction size in bytes (default: 1GB)")]
    public long MaxSizeBytes { get; set; } = 1024L * 1024 * 1024; // 1GB

    /// <summary>
    /// Patterns to include (e.g., ['*.txt', '*.cs']).
    /// </summary>
    [JsonDescription("Patterns to include (e.g., ['*.txt', '*.cs'])")]
    public List<string>? IncludePatterns { get; set; }

    /// <summary>
    /// Patterns to exclude (e.g., ['*.tmp', '.git/*']).
    /// </summary>
    [JsonDescription("Patterns to exclude (e.g., ['*.tmp', '.git/*'])")]
    public List<string>? ExcludePatterns { get; set; }
}

#endregion

#region Archive Tool

/// <summary>
/// Tool for archive operations (create, extract, list, test, info).
/// </summary>
[McpTool("archive_operations", Description = "Execute archive operations with clear parameter naming")]
public class ArchiveOperationsTool : McpToolBase<ArchiveOperationArgs>
{
    private readonly List<string> _allowedDirectories;
    private const long DefaultMaxSizeBytes = 1024L * 1024 * 1024; // 1GB
    private const int DefaultCompressionLevel = 6;

    public ArchiveOperationsTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    /// <inheritdoc />
    public override string Name => "archive_operations";

    /// <inheritdoc />
    public override string? Description =>
        "Execute archive operations with clear parameter naming: " +
        "CREATE (sourcePath + archiveOutputPath), EXTRACT (archivePath + extractToPath), " +
        "LIST/TEST/INFO (archivePath only). Supports ZIP archives with security features " +
        "like path traversal protection, size limits, and pattern filtering. " +
        "Includes dry-run mode and progress tracking. Only works within allowed directories.";

    /// <inheritdoc />
    protected override async Task<ToolResponse> ExecuteAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        try
        {
            return args.Operation.ToLowerInvariant() switch
            {
                "extract" => await ExtractArchiveAsync(args, cancellationToken),
                "create" => await CreateArchiveAsync(args, cancellationToken),
                "list" => await ListArchiveContentsAsync(args, cancellationToken),
                "test" => await TestArchiveIntegrityAsync(args, cancellationToken),
                "info" => await GetArchiveInfoAsync(args, cancellationToken),
                _ => Error($"Unknown archive operation: {args.Operation}")
            };
        }
        catch (Exception ex)
        {
            return Error($"Archive operation failed: {ex.Message}");
        }
    }

    #region Extract Operation

    private async Task<ToolResponse> ExtractArchiveAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(args.ArchivePath) || string.IsNullOrEmpty(args.ExtractToPath))
        {
            return Error("Archive path and extract destination path are required for extract operation");
        }

        try
        {
            // Validate paths
            var validArchivePath = SecurityUtils.ValidatePath(args.ArchivePath, _allowedDirectories);
            var validExtractToPath = SecurityUtils.ValidatePath(args.ExtractToPath, _allowedDirectories);

            if (!File.Exists(validArchivePath))
            {
                return Error($"Archive file not found: {args.ArchivePath}");
            }

            // Determine archive format
            var archiveFormat = GetArchiveFormat(validArchivePath);
            if (archiveFormat != ArchiveFormat.Zip)
            {
                return Error($"Unsupported archive format: {Path.GetExtension(validArchivePath)}");
            }

            // Create target directory if it doesn't exist
            Directory.CreateDirectory(validExtractToPath);

            var options = args.Options ?? new ArchiveOperationOptions();
            var maxSize = options.MaxSizeBytes;
            var overwrite = options.Overwrite;
            var dryRun = options.DryRun;

            var includePatterns = options.IncludePatterns?.Select(Glob.Parse).ToArray();
            var excludePatterns = options.ExcludePatterns?.Select(Glob.Parse).ToArray();

            // Run extraction in background thread since ZipFile operations are synchronous
            return await Task.Run(() =>
            {
                long totalExtractedSize = 0;
                int extractedCount = 0;
                int skippedCount = 0;
                var results = new List<string>();
                var errors = new List<string>();

                using var archive = ZipFile.OpenRead(validArchivePath);

                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Skip directories
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        continue;
                    }

                    // Apply filtering

                    if (!ShouldIncludeFile(entry.FullName, includePatterns, excludePatterns))
                    {
                        skippedCount++;
                        continue;
                    }

                    // Security check for path traversal
                    if (!IsSecureEntryPath(entry.FullName))
                    {
                        errors.Add($"Unsafe path detected: {entry.FullName}");
                        continue;
                    }

                    // Check size limits
                    if (totalExtractedSize + entry.Length > maxSize)
                    {
                        errors.Add($"Extraction size limit exceeded ({maxSize:N0} bytes)");
                        break;
                    }

                    var targetFilePath = Path.Combine(
                        validExtractToPath,
                        entry.FullName.Replace('/', Path.DirectorySeparatorChar)
                    );

                    try
                    {
                        // Check if file exists
                        if (File.Exists(targetFilePath) && !overwrite)
                        {
                            results.Add($"Skipped: {entry.FullName} (file exists)");
                            skippedCount++;
                        }
                        else
                        {
                            if (dryRun)
                            {
                                results.Add($"[DRY RUN] Would extract: {entry.FullName} -> {targetFilePath}");
                            }
                            else
                            {
                                // Create directory if needed
                                var directory = Path.GetDirectoryName(targetFilePath);
                                if (!string.IsNullOrEmpty(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                // Extract file
                                entry.ExtractToFile(targetFilePath, overwrite);
                                results.Add($"Extracted: {entry.FullName}");

                                totalExtractedSize += entry.Length;
                                extractedCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to extract {entry.FullName}: {ex.Message}");
                    }
                }

                var summary = dryRun
                    ? $"[DRY RUN] Would extract {extractedCount} files ({totalExtractedSize:N0} bytes)"
                    : $"Extracted {extractedCount} files ({totalExtractedSize:N0} bytes), skipped {skippedCount}";

                var response = new StringBuilder();
                response.AppendLine(summary);

                if (results.Count > 0)
                {
                    response.AppendLine("\nProcessed files:");
                    foreach (var result in results.Take(20)) // Limit output
                    {
                        response.AppendLine($"  {result}");
                    }
                    if (results.Count > 20)
                    {
                        response.AppendLine($"  ... and {results.Count - 20} more files");
                    }
                }

                if (errors.Count > 0)
                {
                    response.AppendLine("\nErrors:");
                    foreach (var error in errors)
                    {
                        response.AppendLine($"  {error}");
                    }
                }

                return errors.Count == 0 ? Success(response.ToString()) : Error(response.ToString());
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error($"Extract operation failed: {ex.Message}");
        }
    }

    #endregion

    #region Create Operation

    private async Task<ToolResponse> CreateArchiveAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(args.SourcePath) || string.IsNullOrEmpty(args.ArchiveOutputPath))
        {
            return Error("Source path and archive output path are required for create operation");
        }

        try
        {
            var validSourcePath = SecurityUtils.ValidatePath(args.SourcePath, _allowedDirectories);
            var validArchiveOutputPath = SecurityUtils.ValidatePath(args.ArchiveOutputPath, _allowedDirectories);

            if (!Directory.Exists(validSourcePath) && !File.Exists(validSourcePath))
            {
                return Error($"Source path not found: {args.SourcePath}");
            }

            var options = args.Options ?? new ArchiveOperationOptions();
            var dryRun = options.DryRun;
            var compressionLevel = Math.Clamp(options.CompressionLevel, 0, 9);
            var includePatterns = options.IncludePatterns?.Select(Glob.Parse).ToArray();
            var excludePatterns = options.ExcludePatterns?.Select(Glob.Parse).ToArray();

            // Convert compression level to CompressionLevel enum
            var zipCompressionLevel = compressionLevel switch
            {
                0 => CompressionLevel.NoCompression,
                < 5 => CompressionLevel.Fastest,
                < 8 => CompressionLevel.Optimal,
                _ => CompressionLevel.SmallestSize,
            };

            var filesToCompress = new List<(string fullPath, string relativePath)>();

            // Collect files to compress
            if (File.Exists(validSourcePath))
            {
                // Single file
                filesToCompress.Add((validSourcePath, Path.GetFileName(validSourcePath)));
            }
            else
            {
                // Directory - collect all files
                await CollectFilesForCompressionAsync(
                    validSourcePath,
                    validSourcePath,
                    includePatterns,
                    excludePatterns,
                    filesToCompress,
                    cancellationToken
                );
            }

            if (filesToCompress.Count == 0)
            {
                return Error("No files found to compress");
            }

            long totalSize = 0;
            int compressedCount = 0;
            var results = new List<string>();

            if (dryRun)
            {
                foreach (var (fullPath, relativePath) in filesToCompress)
                {
                    var fileInfo = new FileInfo(fullPath);
                    totalSize += fileInfo.Length;
                    results.Add($"[DRY RUN] Would add: {relativePath} ({fileInfo.Length:N0} bytes)");
                }
            }
            else
            {
                // Create the archive
                using var archive = ZipFile.Open(validArchiveOutputPath, ZipArchiveMode.Create);

                foreach (var (fullPath, relativePath) in filesToCompress)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(fullPath);
                        var entry = archive.CreateEntry(relativePath, zipCompressionLevel);

                        using var entryStream = entry.Open();
                        using var fileStream = File.OpenRead(fullPath);
                        await fileStream.CopyToAsync(entryStream, cancellationToken);

                        results.Add($"Added: {relativePath} ({fileInfo.Length:N0} bytes)");
                        totalSize += fileInfo.Length;
                        compressedCount++;
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Failed to add {relativePath}: {ex.Message}");
                    }
                }
            }

            var summary = dryRun
                ? $"[DRY RUN] Would create archive with {filesToCompress.Count} files ({totalSize:N0} bytes)"
                : $"Created archive with {compressedCount} files ({totalSize:N0} bytes)";

            var response = new StringBuilder();
            response.AppendLine(summary);
            response.AppendLine($"Archive path: {args.ArchiveOutputPath}");
            response.AppendLine($"Compression level: {compressionLevel}");

            if (results.Count > 0)
            {
                response.AppendLine("\nProcessed files:");
                foreach (var result in results.Take(20)) // Limit output
                {
                    response.AppendLine($"  {result}");
                }
                if (results.Count > 20)
                {
                    response.AppendLine($"  ... and {results.Count - 20} more files");
                }
            }

            return Success(response.ToString());
        }
        catch (Exception ex)
        {
            return Error($"Create operation failed: {ex.Message}");
        }
    }

    #endregion

    #region List Operation

    private async Task<ToolResponse> ListArchiveContentsAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(args.ArchivePath))
        {
            return Error("Archive path is required for list operation");
        }

        try
        {
            var validArchivePath = SecurityUtils.ValidatePath(args.ArchivePath, _allowedDirectories);

            if (!File.Exists(validArchivePath))
            {
                return Error($"Archive file not found: {args.ArchivePath}");
            }

            var archiveFormat = GetArchiveFormat(validArchivePath);
            if (archiveFormat != ArchiveFormat.Zip)
            {
                return Error($"Archive format {archiveFormat} not supported for listing");
            }

            var options = args.Options ?? new ArchiveOperationOptions();
            var includePatterns = options.IncludePatterns?.Select(Glob.Parse).ToArray();
            var excludePatterns = options.ExcludePatterns?.Select(Glob.Parse).ToArray();

            return await Task.Run(() =>
            {
                long totalSize = 0;
                long totalCompressedSize = 0;
                var entries = new List<string>();

                using var archive = ZipFile.OpenRead(validArchivePath);

                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Apply filtering
                    if (!ShouldIncludeFile(entry.FullName, includePatterns, excludePatterns))
                    {
                        continue;
                    }


                    string modifiedTime;
                    try
                    {
                        modifiedTime = entry.LastWriteTime.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        modifiedTime = "<invalid timestamp>";
                    }

                    var compressionRatio = entry.Length > 0
                        ? (1.0 - (double)entry.CompressedLength / entry.Length) * 100
                        : 0;

                    entries.Add($"{entry.FullName,-50} {entry.Length,12:N0} bytes  {compressionRatio,6:F1}%  {modifiedTime}");

                    totalSize += entry.Length;
                    totalCompressedSize += entry.CompressedLength;
                }

                var overallCompressionRatio = totalSize > 0
                    ? (1.0 - (double)totalCompressedSize / totalSize) * 100
                    : 0;

                var response = new StringBuilder();
                response.AppendLine($"Archive: {args.ArchivePath}");
                response.AppendLine($"Total files: {entries.Count}");
                response.AppendLine($"Total size: {totalSize:N0} bytes");
                response.AppendLine($"Compressed size: {totalCompressedSize:N0} bytes");
                response.AppendLine($"Compression ratio: {overallCompressionRatio:F1}%");
                response.AppendLine();
                response.AppendLine("Contents:");
                response.AppendLine(new string('-', 100));

                foreach (var entry in entries)
                {
                    response.AppendLine(entry);
                }

                return Success(response.ToString());
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error($"List operation failed: {ex.Message}");
        }
    }

    #endregion

    #region Test Operation

    private async Task<ToolResponse> TestArchiveIntegrityAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(args.ArchivePath))
        {
            return Error("Archive path is required for test operation");
        }

        try
        {
            var validArchivePath = SecurityUtils.ValidatePath(args.ArchivePath, _allowedDirectories);

            if (!File.Exists(validArchivePath))
            {
                return Error($"Archive file not found: {args.ArchivePath}");
            }

            var archiveFormat = GetArchiveFormat(validArchivePath);
            if (archiveFormat != ArchiveFormat.Zip)
            {
                return Error($"Archive format {archiveFormat} not supported for testing");
            }

            int testedCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            using var archive = ZipFile.OpenRead(validArchivePath);

            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Test by reading the entry
                    using var stream = entry.Open();
                    var buffer = new byte[8192];
                    long totalRead = 0;

                    while (totalRead < entry.Length)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead == 0)
                        {
                            break;
                        }


                        totalRead += bytesRead;
                    }

                    if (totalRead == entry.Length)
                    {
                        testedCount++;
                    }
                    else
                    {
                        errors.Add($"{entry.FullName}: Size mismatch - expected {entry.Length}, read {totalRead}");
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{entry.FullName}: {ex.Message}");
                    errorCount++;
                }
            }

            var response = new StringBuilder();
            response.AppendLine($"Archive: {args.ArchivePath}");
            response.AppendLine($"Total files: {archive.Entries.Count}");
            response.AppendLine($"Files tested: {testedCount}");
            response.AppendLine($"Files with errors: {errorCount}");
            response.AppendLine();

            if (errorCount == 0)
            {
                response.AppendLine("Archive integrity: OK");
            }
            else
            {
                response.AppendLine("Archive integrity: FAILED");
                response.AppendLine("\nErrors found:");
                foreach (var error in errors)
                {
                    response.AppendLine($"  {error}");
                }
            }

            return errorCount == 0 ? Success(response.ToString()) : Error(response.ToString());
        }
        catch (Exception ex)
        {
            return Error($"Test operation failed: {ex.Message}");
        }
    }

    #endregion

    #region Info Operation

    private async Task<ToolResponse> GetArchiveInfoAsync(ArchiveOperationArgs args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(args.ArchivePath))
        {
            return Error("Archive path is required for info operation");
        }

        try
        {
            var validArchivePath = SecurityUtils.ValidatePath(args.ArchivePath, _allowedDirectories);

            if (!File.Exists(validArchivePath))
            {
                return Error($"Archive file not found: {args.ArchivePath}");
            }

            var fileInfo = new FileInfo(validArchivePath);
            var archiveFormat = GetArchiveFormat(validArchivePath);

            if (archiveFormat != ArchiveFormat.Zip)
            {
                return Error($"Archive format {archiveFormat} not supported for info operation");
            }

            return await Task.Run(() =>
            {
                var response = new StringBuilder();

                using var archive = ZipFile.OpenRead(validArchivePath);

                long totalSize = 0;
                long totalCompressedSize = 0;
                int fileCount = 0;
                int directoryCount = 0;
                var oldestDate = DateTime.MaxValue;
                var newestDate = DateTime.MinValue;

                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        directoryCount++;
                    }
                    else
                    {
                        fileCount++;
                        totalSize += entry.Length;
                        totalCompressedSize += entry.CompressedLength;

                        // Safely handle DateTimeOffset conversion
                        try
                        {
                            var entryDate = entry.LastWriteTime.UtcDateTime;
                            if (entryDate < oldestDate)
                            {
                                oldestDate = entryDate;
                            }


                            if (entryDate > newestDate)
                            {
                                newestDate = entryDate;
                            }

                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Skip invalid dates in archive entries
                        }
                    }
                }

                var compressionRatio = totalSize > 0
                    ? (1.0 - (double)totalCompressedSize / totalSize) * 100
                    : 0;

                response.AppendLine($"Archive: {args.ArchivePath}");
                response.AppendLine($"Format: ZIP");
                response.AppendLine($"Archive Size: {fileInfo.Length:N0} bytes");
                response.AppendLine($"Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                response.AppendLine($"Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                response.AppendLine();
                response.AppendLine($"Contents:");
                response.AppendLine($"  Files: {fileCount:N0}");
                response.AppendLine($"  Directories: {directoryCount:N0}");
                response.AppendLine($"  Uncompressed Size: {totalSize:N0} bytes");
                response.AppendLine($"  Compressed Size: {totalCompressedSize:N0} bytes");
                response.AppendLine($"  Compression Ratio: {compressionRatio:F1}%");

                if (oldestDate != DateTime.MaxValue && newestDate != DateTime.MinValue)
                {
                    response.AppendLine();
                    response.AppendLine($"File dates:");
                    response.AppendLine($"  Oldest: {oldestDate:yyyy-MM-dd HH:mm:ss} UTC");
                    response.AppendLine($"  Newest: {newestDate:yyyy-MM-dd HH:mm:ss} UTC");
                }
                else if (fileCount > 0)
                {
                    response.AppendLine();
                    response.AppendLine("File dates: Unable to determine (invalid timestamps in archive)");
                }

                return Success(response.ToString());
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return Error($"Info operation failed: {ex.Message}");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Collects files for compression recursively.
    /// </summary>
    private async Task CollectFilesForCompressionAsync(
        string currentPath,
        string basePath,
        Glob[]? includePatterns,
        Glob[]? excludePatterns,
        List<(string fullPath, string relativePath)> results,
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                var files = Directory.EnumerateFiles(currentPath);
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relativePath = Path.GetRelativePath(basePath, file);

                    if (ShouldIncludeFile(relativePath, includePatterns, excludePatterns))
                    {
                        results.Add((file, relativePath));
                    }
                }

                var directories = Directory.EnumerateDirectories(currentPath);
                var tasks = new List<Task>();

                foreach (var directory in directories)
                {
                    tasks.Add(CollectFilesForCompressionAsync(
                        directory,
                        basePath,
                        includePatterns,
                        excludePatterns,
                        results,
                        cancellationToken
                    ));
                }

                Task.WaitAll(tasks.ToArray(), cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Determines archive format from file extension.
    /// </summary>
    private static ArchiveFormat GetArchiveFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".zip" => ArchiveFormat.Zip,
            ".tar" => ArchiveFormat.Tar,
            ".gz" => ArchiveFormat.GZip,
            ".7z" => ArchiveFormat.SevenZip,
            ".rar" => ArchiveFormat.Rar,
            _ => ArchiveFormat.Unsupported,
        };
    }

    /// <summary>
    /// Checks if a file should be included based on patterns.
    /// </summary>
    private static bool ShouldIncludeFile(
        string filePath,
        Glob[]? includePatterns,
        Glob[]? excludePatterns)
    {
        // Check exclude patterns first
        if (excludePatterns != null && excludePatterns.Any(pattern => pattern.IsMatch(filePath)))
        {
            return false;
        }

        // Check include patterns

        if (includePatterns != null && includePatterns.Length > 0)
        {

            return includePatterns.Any(pattern => pattern.IsMatch(filePath));
        }


        return true;
    }

    /// <summary>
    /// Checks if an archive entry path is secure (no path traversal).
    /// </summary>
    private static bool IsSecureEntryPath(string entryPath)
    {
        // Normalize path separators
        var normalizedPath = entryPath.Replace('\\', '/');

        // Check for path traversal attempts
        if (normalizedPath.Contains("../") ||
            normalizedPath.Contains("..\\") ||
            normalizedPath.StartsWith("/") ||
            normalizedPath.StartsWith("\\") ||
            Path.IsPathRooted(normalizedPath))
        {
            return false;
        }

        // Check for dangerous filenames
        var fileName = Path.GetFileName(normalizedPath);
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Contains(":") ||
            fileName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
        {
            return false;
        }

        return true;
    }

    #endregion
}

#endregion

#region Supporting Types

/// <summary>
/// Supported archive formats.
/// </summary>
internal enum ArchiveFormat
{
    Unsupported,
    Zip,
    Tar,
    GZip,
    SevenZip,
    Rar,
}

#endregion
