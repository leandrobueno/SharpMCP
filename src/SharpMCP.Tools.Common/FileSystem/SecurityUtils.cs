namespace SharpMCP.Tools.Common.FileSystem;

/// <summary>
/// Security utilities for file system operations.
/// </summary>
public static class SecurityUtils
{
    /// <summary>
    /// Validates a path to ensure it's within allowed directories.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="allowedDirectories">List of allowed directory roots.</param>
    /// <returns>The validated absolute path.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when path is outside allowed directories.</exception>
    public static string ValidatePath(string path, List<string> allowedDirectories)
    {
        if (string.IsNullOrWhiteSpace(path))
        {

            throw new ArgumentException("Path cannot be null or empty");
        }

        // Get the absolute path

        var fullPath = Path.GetFullPath(path);

        // Normalize paths for comparison
        var normalizedPath = NormalizePath(fullPath);

        // Check if the path is within any allowed directory
        foreach (var allowedDir in allowedDirectories)
        {
            var normalizedAllowedDir = NormalizePath(Path.GetFullPath(allowedDir));

            if (normalizedPath.StartsWith(normalizedAllowedDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }
        }

        throw new UnauthorizedAccessException($"Access denied: Path '{path}' is outside allowed directories");
    }

    /// <summary>
    /// Normalizes a path for comparison.
    /// </summary>
    private static string NormalizePath(string path)
    {
        // Ensure path ends with directory separator for proper prefix matching
        path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return path + Path.DirectorySeparatorChar;
    }
}
