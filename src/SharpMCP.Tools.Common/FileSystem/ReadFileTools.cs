using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;

namespace SharpMCP.Tools.Common.FileSystem;

/// <summary>
/// Arguments for reading a single file.
/// </summary>
public class ReadFileArgs
{
    /// <summary>
    /// Path to the file to read.
    /// </summary>
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the file to read")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Tool for reading file contents.
/// </summary>
[McpTool("read_file", Description = "Read the complete contents of a file from the file system")]
public class ReadFileTool : McpToolBase<ReadFileArgs>
{
    private readonly List<string> _allowedDirectories;

    public ReadFileTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    /// <inheritdoc />
    public override string Name => "read_file";

    /// <inheritdoc />
    public override string? Description =>
        "Read the complete contents of a file from the file system. " +
        "Handles various text encodings and provides detailed error messages " +
        "if the file cannot be read. Use this tool when you need to examine " +
        "the contents of a single file. Only works within allowed directories.";

    /// <inheritdoc />
    protected override async Task<ToolResponse> ExecuteAsync(ReadFileArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);
            var content = await File.ReadAllTextAsync(validPath, cancellationToken);
            return Success(content);
        }
        catch (Exception ex)
        {
            return Error($"Error reading file: {ex.Message}");
        }
    }
}

/// <summary>
/// Arguments for reading multiple files.
/// </summary>
public class ReadMultipleFilesArgs
{
    /// <summary>
    /// Array of file paths to read.
    /// </summary>
    [Core.Schema.JsonRequired]
    [JsonDescription("Array of file paths to read")]
    public List<string> Paths { get; set; } = [];
}

/// <summary>
/// Tool for reading multiple files simultaneously.
/// </summary>
[McpTool("read_multiple_files", Description = "Read the contents of multiple files simultaneously")]
public class ReadMultipleFilesTool : McpToolBase<ReadMultipleFilesArgs>
{
    private readonly List<string> _allowedDirectories;
    private const int MaxParallelism = 8;

    public ReadMultipleFilesTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    /// <inheritdoc />
    public override string Name => "read_multiple_files";

    /// <inheritdoc />
    public override string? Description =>
        "Read the contents of multiple files simultaneously. This is more " +
        "efficient than reading files one by one when you need to analyze " +
        "or compare multiple files. Each file's content is returned with its " +
        "path as a reference. Failed reads for individual files won't stop " +
        "the entire operation. Only works within allowed directories.";

    /// <inheritdoc />
    protected override async Task<ToolResponse> ExecuteAsync(ReadMultipleFilesArgs args, CancellationToken cancellationToken)
    {
        var results = new List<string>();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Min(MaxParallelism, args.Paths.Count),
            CancellationToken = cancellationToken
        };

        var orderedResults = new SortedDictionary<int, string>();
        var lockObj = new object();

        await Parallel.ForEachAsync(
            args.Paths.Select((path, index) => (path, index)),
            parallelOptions,
            async (item, ct) =>
            {
                try
                {
                    var validPath = SecurityUtils.ValidatePath(item.path, _allowedDirectories);
                    var content = await File.ReadAllTextAsync(validPath, ct);
                    lock (lockObj)
                    {
                        orderedResults[item.index] = $"{item.path}:\n{content}\n";
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObj)
                    {
                        orderedResults[item.index] = $"{item.path}: Error - {ex.Message}";
                    }
                }
            });

        var combinedResult = string.Join("\n---\n", orderedResults.Values);
        return Success(combinedResult);
    }
}
