using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;

namespace SharpMCP.Tools.Common.FileSystem;

/// <summary>
/// Arguments for writing a file.
/// </summary>
public class WriteFileArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the file to write")]
    public string Path { get; set; } = string.Empty;

    [Core.Schema.JsonRequired]
    [JsonDescription("Content to write to the file")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Tool for writing file contents.
/// </summary>
[McpTool("write_file", Description = "Create a new file or completely overwrite an existing file")]
public class WriteFileTool : McpToolBase<WriteFileArgs>
{
    private readonly List<string> _allowedDirectories;

    public WriteFileTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "write_file";

    public override string? Description =>
        "Create a new file or completely overwrite an existing file with new content. " +
        "Use with caution as it will overwrite existing files without warning. " +
        "Handles text content with proper encoding. Only works within allowed directories.";

    protected override async Task<ToolResponse> ExecuteAsync(WriteFileArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);
            await File.WriteAllTextAsync(validPath, args.Content, cancellationToken);
            return Success($"Successfully wrote to {args.Path}");
        }
        catch (Exception ex)
        {
            return Error($"Error writing file: {ex.Message}");
        }
    }
}

/// <summary>
/// Arguments for creating a directory.
/// </summary>
public class CreateDirectoryArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the directory to create")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Tool for creating directories.
/// </summary>
[McpTool("create_directory", Description = "Create a new directory or ensure a directory exists")]
public class CreateDirectoryTool : McpToolBase<CreateDirectoryArgs>
{
    private readonly List<string> _allowedDirectories;

    public CreateDirectoryTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "create_directory";

    public override string? Description =>
        "Create a new directory or ensure a directory exists. Can create multiple " +
        "nested directories in one operation. If the directory already exists, " +
        "this operation will succeed silently. Perfect for setting up directory " +
        "structures for projects or ensuring required paths exist. Only works within allowed directories.";

    protected override Task<ToolResponse> ExecuteAsync(CreateDirectoryArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);
            Directory.CreateDirectory(validPath);
            return Task.FromResult(Success($"Successfully created directory {args.Path}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Error($"Error creating directory: {ex.Message}"));
        }
    }
}

/// <summary>
/// Arguments for moving/renaming files.
/// </summary>
public class MoveFileArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Source path")]
    public string Source { get; set; } = string.Empty;

    [Core.Schema.JsonRequired]
    [JsonDescription("Destination path")]
    public string Destination { get; set; } = string.Empty;
}

/// <summary>
/// Tool for moving or renaming files and directories.
/// </summary>
[McpTool("move_file", Description = "Move or rename files and directories")]
public class MoveFileTool : McpToolBase<MoveFileArgs>
{
    private readonly List<string> _allowedDirectories;

    public MoveFileTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "move_file";

    public override string? Description =>
        "Move or rename files and directories. Can move files between directories " +
        "and rename them in a single operation. If the destination exists, the " +
        "operation will fail. Works across different directories and can be used " +
        "for simple renaming within the same directory. Both source and destination must be within allowed directories.";

    protected override Task<ToolResponse> ExecuteAsync(MoveFileArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validSource = SecurityUtils.ValidatePath(args.Source, _allowedDirectories);
            var validDestination = SecurityUtils.ValidatePath(args.Destination, _allowedDirectories);

            if (File.Exists(validSource))
            {
                File.Move(validSource, validDestination);
            }
            else if (Directory.Exists(validSource))
            {
                Directory.Move(validSource, validDestination);
            }
            else
            {
                throw new FileNotFoundException($"Source not found: {args.Source}");
            }

            return Task.FromResult(Success($"Successfully moved {args.Source} to {args.Destination}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Error($"Error moving file: {ex.Message}"));
        }
    }
}
