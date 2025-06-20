# SharpMCP.Tools.Common API Reference

Common tool implementations and utilities for file system operations.

## File System Tools

### ReadFileTool

Reads content from a single file.

```csharp
[McpTool("read_file", Description = "Read the complete contents of a file from the file system")]
public class ReadFileTool : McpToolBase<ReadFileArgs>
{
    public ReadFileTool(List<string>? allowedDirectories = null);
    
    public override string Name => "read_file";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(ReadFileArgs args, CancellationToken cancellationToken);
}

public class ReadFileArgs
{
    [JsonRequired]
    [JsonDescription("Path to the file to read")]
    public string Path { get; set; } = "";
}
```

### ReadMultipleFilesTool

Reads content from multiple files simultaneously.

```csharp
[McpTool("read_multiple_files", Description = "Read the contents of multiple files simultaneously")]
public class ReadMultipleFilesTool : McpToolBase<ReadMultipleFilesArgs>
{
    public ReadMultipleFilesTool(List<string>? allowedDirectories = null);
    
    public override string Name => "read_multiple_files";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(ReadMultipleFilesArgs args, CancellationToken cancellationToken);
}

public class ReadMultipleFilesArgs
{
    [JsonRequired]
    [JsonDescription("Array of file paths to read")]
    public List<string> Paths { get; set; } = [];
}
```

### WriteFileTool

Creates or overwrites files with new content.

```csharp
[McpTool("write_file", Description = "Create a new file or completely overwrite an existing file")]
public class WriteFileTool : McpToolBase<WriteFileArgs>
{
    public WriteFileTool(List<string>? allowedDirectories = null);
    
    public override string Name => "write_file";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(WriteFileArgs args, CancellationToken cancellationToken);
}

public class WriteFileArgs
{
    [JsonRequired]
    [JsonDescription("Path to the file to write")]
    public string Path { get; set; } = "";

    [JsonRequired]
    [JsonDescription("Content to write to the file")]
    public string Content { get; set; } = "";
}
```

### CreateDirectoryTool

Creates directories and nested directory structures.

```csharp
[McpTool("create_directory", Description = "Create a new directory or ensure a directory exists")]
public class CreateDirectoryTool : McpToolBase<CreateDirectoryArgs>
{
    public CreateDirectoryTool(List<string>? allowedDirectories = null);
    
    public override string Name => "create_directory";
    public override string? Description { get; }
    
    protected override Task<ToolResponse> ExecuteAsync(CreateDirectoryArgs args, CancellationToken cancellationToken);
}

public class CreateDirectoryArgs
{
    [JsonRequired]
    [JsonDescription("Path to the directory to create")]
    public string Path { get; set; } = "";
}
```

### MoveFileTool

Moves or renames files and directories.

```csharp
[McpTool("move_file", Description = "Move or rename files and directories")]
public class MoveFileTool : McpToolBase<MoveFileArgs>
{
    public MoveFileTool(List<string>? allowedDirectories = null);
    
    public override string Name => "move_file";
    public override string? Description { get; }
    
    protected override Task<ToolResponse> ExecuteAsync(MoveFileArgs args, CancellationToken cancellationToken);
}

public class MoveFileArgs
{
    [JsonRequired]
    [JsonDescription("Source path")]
    public string Source { get; set; } = "";

    [JsonRequired]
    [JsonDescription("Destination path")]
    public string Destination { get; set; } = "";
}
```

## Directory Operations

### ListDirectoryTool

Lists contents of directories with file/directory indicators.

```csharp
[McpTool("list_directory", Description = "Get a detailed listing of all files and directories in a specified path")]
public class ListDirectoryTool : McpToolBase<ListDirectoryArgs>
{
    public ListDirectoryTool(List<string>? allowedDirectories = null);
    
    public override string Name => "list_directory";
    public override string? Description { get; }
    
    protected override Task<ToolResponse> ExecuteAsync(ListDirectoryArgs args, CancellationToken cancellationToken);
}

public class ListDirectoryArgs
{
    [JsonRequired]
    [JsonDescription("Path to the directory to list")]
    public string Path { get; set; } = "";
}
```

### DirectoryTreeTool

Creates recursive tree view of directory structures.

```csharp
[McpTool("directory_tree", Description = "Get a recursive tree view of files and directories as a JSON structure")]
public class DirectoryTreeTool : McpToolBase<DirectoryTreeArgs>
{
    public DirectoryTreeTool(List<string>? allowedDirectories = null);
    
    public override string Name => "directory_tree";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(DirectoryTreeArgs args, CancellationToken cancellationToken);
}

public class DirectoryTreeArgs
{
    [JsonRequired]
    [JsonDescription("Path to the directory to create tree from")]
    public string Path { get; set; } = "";
}

public class TreeEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("children")]
    public List<TreeEntry>? Children { get; set; }
}
```

## Search and Discovery

### SearchFilesTool

Recursively searches for files matching patterns.

```csharp
[McpTool("search_files", Description = "Recursively search for files and directories matching a pattern")]
public class SearchFilesTool : McpToolBase<SearchFilesArgs>
{
    public SearchFilesTool(List<string>? allowedDirectories = null);
    
    public override string Name => "search_files";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(SearchFilesArgs args, CancellationToken cancellationToken);
}

public class SearchFilesArgs
{
    [JsonRequired]
    [JsonDescription("Starting directory for search")]
    public string Path { get; set; } = "";

    [JsonRequired]
    [JsonDescription("Search pattern - can be simple text, wildcard (* and ?), or regex")]
    public string Pattern { get; set; } = "";

    [JsonDescription("Type of pattern: 'simple' (default), 'wildcard', or 'regex'")]
    [JsonPropertyName("patternType")]
    public string PatternType { get; set; } = "simple";

    [JsonDescription("Patterns to exclude from search")]
    [JsonPropertyName("excludePatterns")]
    public List<string>? ExcludePatterns { get; set; }
}

public enum SearchPatternType
{
    Simple,
    Wildcard,
    Regex
}
```

### GetFileInfoTool

Retrieves detailed metadata about files and directories.

```csharp
[McpTool("get_file_info", Description = "Retrieve detailed metadata about a file or directory")]
public class GetFileInfoTool : McpToolBase<GetFileInfoArgs>
{
    public GetFileInfoTool(List<string>? allowedDirectories = null);
    
    public override string Name => "get_file_info";
    public override string? Description { get; }
    
    protected override Task<ToolResponse> ExecuteAsync(GetFileInfoArgs args, CancellationToken cancellationToken);
}

public class GetFileInfoArgs
{
    [JsonRequired]
    [JsonDescription("Path to the file or directory")]
    public string Path { get; set; } = "";
}
```

### ListAllowedDirectoriesTool

Returns the list of allowed directories for security reference.

```csharp
[McpTool("list_allowed_directories", Description = "Returns the list of directories that this server is allowed to access")]
public class ListAllowedDirectoriesTool : McpToolBase
{
    public ListAllowedDirectoriesTool(List<string>? allowedDirectories = null);
    
    public override string Name => "list_allowed_directories";
    public override string? Description { get; }
    
    protected override Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken);
}
```

## Archive Operations

### ArchiveOperationsTool

Comprehensive archive operations for ZIP files.

```csharp
[McpTool("archive_operations", Description = "Execute archive operations with clear parameter naming")]
public class ArchiveOperationsTool : McpToolBase<ArchiveOperationArgs>
{
    public ArchiveOperationsTool(List<string>? allowedDirectories = null);
    
    public override string Name => "archive_operations";
    public override string? Description { get; }
    
    protected override async Task<ToolResponse> ExecuteAsync(ArchiveOperationArgs args, CancellationToken cancellationToken);
}

public class ArchiveOperationArgs
{
    [JsonRequired]
    [JsonDescription("Type of archive operation: 'extract', 'create', 'list', 'test', 'info'")]
    public string Operation { get; set; } = "";

    [JsonDescription("Path to existing archive file (for extract/list/test/info operations)")]
    public string? ArchivePath { get; set; }

    [JsonDescription("Source path for files/directory to compress (for create operation)")]
    public string? SourcePath { get; set; }

    [JsonDescription("Path where new archive should be created (for create operation)")]
    public string? ArchiveOutputPath { get; set; }

    [JsonDescription("Directory where files should be extracted (for extract operation)")]
    public string? ExtractToPath { get; set; }

    [JsonDescription("Archive operation options")]
    public ArchiveOperationOptions? Options { get; set; }
}

public class ArchiveOperationOptions
{
    [JsonDescription("Overwrite existing files during extraction (default: false)")]
    public bool Overwrite { get; set; } = false;

    [JsonDescription("Preserve file permissions during extraction (default: true)")]
    public bool PreservePermissions { get; set; } = true;

    [JsonDescription("Compression level 0-9 for creating archives (default: 6)")]
    public int CompressionLevel { get; set; } = 6;

    [JsonDescription("Preview operations without executing (default: false)")]
    public bool DryRun { get; set; } = false;

    [JsonDescription("Maximum extraction size in bytes (default: 1GB)")]
    public long MaxSizeBytes { get; set; } = 1024L * 1024 * 1024;

    [JsonDescription("Patterns to include (e.g., ['*.txt', '*.cs'])")]
    public List<string>? IncludePatterns { get; set; }

    [JsonDescription("Patterns to exclude (e.g., ['*.tmp', '.git/*'])")]
    public List<string>? ExcludePatterns { get; set; }
}
```

## Security Utilities

### SecurityUtils

Path validation and security utilities.

```csharp
public static class SecurityUtils
{
    public static string ValidatePath(string path, List<string> allowedDirectories);
    public static bool IsPathAllowed(string path, List<string> allowedDirectories);
    public static string NormalizePath(string path);
    public static bool IsSecurePath(string path);
    public static void ValidateFileExtension(string path, List<string> allowedExtensions);
    public static void ValidateFileSize(long fileSize, long maxSize);
}
```

## Registration Extensions

### CommonTools

Static class for bulk tool registration.

```csharp
public static class CommonTools
{
    public static void RegisterFileSystemTools(IMcpServer server, List<string>? allowedDirectories = null);
}
```

### ServerBuilderExtensions

Extension methods for easy tool registration.

```csharp
public static class ServerBuilderExtensions
{
    public static IMcpServerBuilder AddFileSystemTools(this IMcpServerBuilder builder, List<string>? allowedDirectories = null);
}
```

## Performance Configuration

### FileSystemConfig

Configuration for file system operations.

```csharp
public class FileSystemConfig
{
    public List<string> AllowedDirectories { get; set; } = [];
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public int MaxConcurrentReads { get; set; } = 8;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public List<string> AllowedExtensions { get; set; } = [];
    public List<string> BlockedExtensions { get; set; } = [".exe", ".dll", ".bat"];
}
```

## Constants

### Common File Extensions

```csharp
public static class FileExtensions
{
    public static readonly string[] Text = [".txt", ".md", ".csv", ".log"];
    public static readonly string[] Code = [".cs", ".js", ".py", ".json", ".xml"];
    public static readonly string[] Archive = [".zip", ".tar", ".gz", ".7z"];
    public static readonly string[] Unsafe = [".exe", ".dll", ".bat", ".cmd", ".ps1"];
}
```

### Archive Formats

```csharp
internal enum ArchiveFormat
{
    Unsupported,
    Zip,
    Tar,
    GZip,
    SevenZip,
    Rar
}
```

## Error Handling

### FileSystemException

Specialized exceptions for file system operations.

```csharp
public class FileSystemException : McpToolException
{
    public string Path { get; }
    
    public FileSystemException(string message, string path);
    public FileSystemException(string message, string path, Exception innerException);
}
```

### SecurityException

Security-related exceptions for path validation.

```csharp
public class SecurityException : McpToolException
{
    public string AttemptedPath { get; }
    
    public SecurityException(string message, string attemptedPath);
    public SecurityException(string message, string attemptedPath, Exception innerException);
}
```
