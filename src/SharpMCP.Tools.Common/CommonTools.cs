using SharpMCP.Core.Server;
using SharpMCP.Tools.Common.FileSystem;

namespace SharpMCP.Tools.Common;

/// <summary>
/// Collection of common tools for SharpMCP.
/// </summary>
public static class CommonTools
{
    /// <summary>
    /// Registers all common file system tools with the server.
    /// </summary>
    /// <param name="server">The MCP server to register tools with.</param>
    /// <param name="allowedDirectories">Optional list of allowed directories. Defaults to current directory.</param>
    public static void RegisterFileSystemTools(IMcpServer server, List<string>? allowedDirectories = null)
    {
        var dirs = allowedDirectories ?? [Directory.GetCurrentDirectory()];

        // Read operations
        server.RegisterTool(new ReadFileTool(dirs));
        server.RegisterTool(new ReadMultipleFilesTool(dirs));

        // Write operations
        server.RegisterTool(new WriteFileTool(dirs));
        server.RegisterTool(new CreateDirectoryTool(dirs));
        server.RegisterTool(new MoveFileTool(dirs));

        // Directory operations
        server.RegisterTool(new ListDirectoryTool(dirs));
        server.RegisterTool(new DirectoryTreeTool(dirs));

        // Search and info operations
        server.RegisterTool(new SearchFilesTool(dirs));
        server.RegisterTool(new GetFileInfoTool(dirs));
        server.RegisterTool(new ListAllowedDirectoriesTool(dirs));

        // Archive operations
        server.RegisterTool(new ArchiveOperationsTool(dirs));
    }

    /// <summary>
    /// Extension method to easily add file system tools to a server builder.
    /// </summary>
    public static IMcpServerBuilder AddFileSystemTools(this IMcpServerBuilder builder, List<string>? allowedDirectories = null)
    {
        var dirs = allowedDirectories ?? [Directory.GetCurrentDirectory()];

        return builder
            .AddTool(new ReadFileTool(dirs))
            .AddTool(new ReadMultipleFilesTool(dirs))
            .AddTool(new WriteFileTool(dirs))
            .AddTool(new CreateDirectoryTool(dirs))
            .AddTool(new MoveFileTool(dirs))
            .AddTool(new ListDirectoryTool(dirs))
            .AddTool(new DirectoryTreeTool(dirs))
            .AddTool(new SearchFilesTool(dirs))
            .AddTool(new GetFileInfoTool(dirs))
            .AddTool(new ListAllowedDirectoriesTool(dirs))
            .AddTool(new ArchiveOperationsTool(dirs));
    }
}
