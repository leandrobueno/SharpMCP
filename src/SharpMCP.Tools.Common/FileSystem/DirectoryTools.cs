using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;
using System.Text.Json.Serialization;

namespace SharpMCP.Tools.Common.FileSystem;

/// <summary>
/// Arguments for listing a directory.
/// </summary>
public class ListDirectoryArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the directory to list")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Tool for listing directory contents.
/// </summary>
[McpTool("list_directory", Description = "Get a detailed listing of all files and directories in a specified path")]
public class ListDirectoryTool : McpToolBase<ListDirectoryArgs>
{
    private readonly List<string> _allowedDirectories;

    public ListDirectoryTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "list_directory";

    public override string? Description =>
        "Get a detailed listing of all files and directories in a specified path. " +
        "Results clearly distinguish between files and directories with [FILE] and [DIR] " +
        "prefixes. This tool is essential for understanding directory structure and " +
        "finding specific files within a directory. Only works within allowed directories.";

    protected override Task<ToolResponse> ExecuteAsync(ListDirectoryArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);

            var entries = Directory
                .EnumerateFileSystemEntries(validPath)
                .Select(entry =>
                {
                    var info = new FileInfo(entry);
                    var isDirectory = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
                    return $"{(isDirectory ? "[DIR]" : "[FILE]")} {Path.GetFileName(entry)}";
                })
                .OrderBy(e => e);

            return Task.FromResult(Success(string.Join("\n", entries)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Error($"Error listing directory: {ex.Message}"));
        }
    }
}

/// <summary>
/// Tree entry for directory tree output.
/// </summary>
public class TreeEntry
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("children")]
    public List<TreeEntry>? Children { get; set; }
}

/// <summary>
/// Arguments for directory tree.
/// </summary>
public class DirectoryTreeArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the directory to create tree from")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Tool for getting directory tree structure.
/// </summary>
[McpTool("directory_tree", Description = "Get a recursive tree view of files and directories as a JSON structure")]
public class DirectoryTreeTool : McpToolBase<DirectoryTreeArgs>
{
    private readonly List<string> _allowedDirectories;
    private const int MaxParallelism = 8;

    public DirectoryTreeTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "directory_tree";

    public override string? Description =>
        "Get a recursive tree view of files and directories as a JSON structure. " +
        "Each entry includes 'name', 'type' (file/directory), and 'children' for directories. " +
        "Files have no children array, while directories always have a children array (which may be empty). " +
        "The output is formatted with 2-space indentation for readability. Only works within allowed directories.";

    protected override async Task<ToolResponse> ExecuteAsync(DirectoryTreeArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var semaphore = new SemaphoreSlim(MaxParallelism);
            var tree = await BuildTreeAsync(args.Path, semaphore, cancellationToken);
            var json = System.Text.Json.JsonSerializer.Serialize(tree, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
            return Success(json);
        }
        catch (Exception ex)
        {
            return Error($"Error building directory tree: {ex.Message}");
        }
    }

    private async Task<List<TreeEntry>> BuildTreeAsync(string path, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        var validPath = SecurityUtils.ValidatePath(path, _allowedDirectories);
        var entries = Directory.EnumerateFileSystemEntries(validPath).ToList();
        var treeEntries = new List<TreeEntry>();

        var tasks = entries.Select(async entry =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var info = new FileInfo(entry);
                var isDirectory = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory;

                var treeEntry = new TreeEntry
                {
                    Name = Path.GetFileName(entry),
                    Type = isDirectory ? "directory" : "file"
                };

                if (isDirectory)
                {
                    treeEntry.Children = await BuildTreeAsync(entry, semaphore, cancellationToken);
                }

                return treeEntry;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results
            .OrderBy(e => e.Type == "directory" ? 0 : 1)
            .ThenBy(e => e.Name)
            .ToList();
    }
}
