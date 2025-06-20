using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SharpMCP.Tools.Common.FileSystem;

/// <summary>
/// Search pattern type.
/// </summary>
public enum SearchPatternType
{
    Simple,
    Wildcard,
    Regex
}

/// <summary>
/// Arguments for searching files.
/// </summary>
public class SearchFilesArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Starting directory for search")]
    public string Path { get; set; } = string.Empty;

    [Core.Schema.JsonRequired]
    [JsonDescription("Search pattern - can be simple text, wildcard (* and ?), or regex (if patternType is set)")]
    public string Pattern { get; set; } = string.Empty;

    [JsonDescription("Type of pattern: 'simple' (default), 'wildcard', or 'regex'")]
    [JsonPropertyName("patternType")]
    public string PatternType { get; set; } = "simple";

    [JsonDescription("Patterns to exclude from search")]
    [JsonPropertyName("excludePatterns")]
    public List<string>? ExcludePatterns { get; set; }
}

/// <summary>
/// Tool for searching files and directories.
/// </summary>
[McpTool("search_files", Description = "Recursively search for files and directories matching a pattern")]
public class SearchFilesTool : McpToolBase<SearchFilesArgs>
{
    private readonly List<string> _allowedDirectories;
    private readonly Dictionary<string, Regex> _regexCache = [];

    public SearchFilesTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "search_files";

    public override string? Description =>
        "Recursively search for files and directories matching a pattern. " +
        "Supports three pattern types: 'simple' (default - partial text match), " +
        "'wildcard' (using * and ? wildcards), or 'regex' (full regular expressions). " +
        "Searches through all subdirectories from the starting path. The search " +
        "is case-insensitive. Returns full paths to all matching items. " +
        "Great for finding files when you don't know their exact location. " +
        "Only searches within allowed directories.";

    protected override async Task<ToolResponse> ExecuteAsync(SearchFilesArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);
            var patternType = Enum.TryParse<SearchPatternType>(args.PatternType, true, out var type)
                ? type : SearchPatternType.Simple;

            var regex = CreateSearchRegex(args.Pattern, patternType);
            var excludePatterns = args.ExcludePatterns ?? [];
            var results = new List<string>();

            await SearchRecursive(validPath, validPath, regex, excludePatterns, results, cancellationToken);

            var sortedResults = results.OrderBy(r => r).ToList();
            return Success(sortedResults.Count > 0
                ? string.Join("\n", sortedResults)
                : "No matches found");
        }
        catch (Exception ex)
        {
            return Error($"Error during search: {ex.Message}");
        }
    }

    private async Task SearchRecursive(string currentPath, string rootPath, Regex regex,
        List<string> excludePatterns, List<string> results, CancellationToken cancellationToken)
    {
        try
        {
            SecurityUtils.ValidatePath(currentPath, _allowedDirectories);
        }
        catch
        {
            return; // Skip invalid paths
        }

        IEnumerable<string> entries;
        try
        {
            entries = Directory.EnumerateFileSystemEntries(currentPath);
        }
        catch
        {
            return; // Skip inaccessible directories
        }

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(rootPath, entry);
            var shouldExclude = excludePatterns.Any(excludePattern =>
            {
                var pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return pathParts.Any(part => part.Equals(excludePattern, StringComparison.OrdinalIgnoreCase));
            });

            if (shouldExclude)
            {
                continue;
            }


            var fileName = Path.GetFileName(entry);
            if (regex.IsMatch(fileName))
            {
                results.Add(entry);
            }

            if (Directory.Exists(entry))
            {
                await SearchRecursive(entry, rootPath, regex, excludePatterns, results, cancellationToken);
            }
        }
    }

    private Regex CreateSearchRegex(string pattern, SearchPatternType patternType)
    {
        var cacheKey = $"{patternType}:{pattern.ToLowerInvariant()}";

        if (_regexCache.TryGetValue(cacheKey, out var cached))
        {

            return cached;
        }


        var regex = patternType switch
        {
            SearchPatternType.Wildcard => CreateWildcardRegex(pattern),
            SearchPatternType.Regex => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled),
            _ => new Regex(Regex.Escape(pattern), RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        if (_regexCache.Count > 100)
        {
            _regexCache.Clear();
        }


        _regexCache[cacheKey] = regex;
        return regex;
    }

    private static Regex CreateWildcardRegex(string wildcardPattern)
    {
        var escapedPattern = Regex.Escape(wildcardPattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".");
        var regexPattern = $"^{escapedPattern}$";
        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

/// <summary>
/// Arguments for getting file info.
/// </summary>
public class GetFileInfoArgs
{
    [Core.Schema.JsonRequired]
    [JsonDescription("Path to the file or directory")]
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// Tool for getting file information.
/// </summary>
[McpTool("get_file_info", Description = "Retrieve detailed metadata about a file or directory")]
public class GetFileInfoTool : McpToolBase<GetFileInfoArgs>
{
    private readonly List<string> _allowedDirectories;

    public GetFileInfoTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "get_file_info";

    public override string? Description =>
        "Retrieve detailed metadata about a file or directory. Returns comprehensive " +
        "information including size, creation time, last modified time, permissions, " +
        "and type. This tool is perfect for understanding file characteristics " +
        "without reading the actual content. Only works within allowed directories.";

    protected override Task<ToolResponse> ExecuteAsync(GetFileInfoArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var validPath = SecurityUtils.ValidatePath(args.Path, _allowedDirectories);
            var info = new FileInfo(validPath);

            var details = new[]
            {
                $"size: {(info.Exists ? info.Length : 0)}",
                $"created: {info.CreationTime}",
                $"modified: {info.LastWriteTime}",
                $"accessed: {info.LastAccessTime}",
                $"isDirectory: {info.Attributes.HasFlag(FileAttributes.Directory)}",
                $"isFile: {!info.Attributes.HasFlag(FileAttributes.Directory)}",
                $"permissions: {Convert.ToString((int)info.Attributes, 8)}"
            };

            return Task.FromResult(Success(string.Join("\n", details)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Error($"Error getting file info: {ex.Message}"));
        }
    }
}

/// <summary>
/// Tool for listing allowed directories.
/// </summary>
[McpTool("list_allowed_directories", Description = "Returns the list of directories that this server is allowed to access")]
public class ListAllowedDirectoriesTool : McpToolBase
{
    private readonly List<string> _allowedDirectories;

    public ListAllowedDirectoriesTool(List<string>? allowedDirectories = null)
    {
        _allowedDirectories = allowedDirectories ?? [Directory.GetCurrentDirectory()];
    }

    public override string Name => "list_allowed_directories";

    public override string? Description =>
        "Returns the list of directories that this server is allowed to access. " +
        "Use this to understand which directories are available before trying to access files.";

    protected override Task<ToolResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = $"Allowed directories:\n{string.Join("\n", _allowedDirectories)}";
        return Task.FromResult(Success(result));
    }
}
