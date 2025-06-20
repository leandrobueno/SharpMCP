using SharpMCP.Server;
using SharpMCP.Tools.Common;

// Parse command line arguments for allowed directories
// If no directories specified, use current directory
var allowedDirectories = args.Length > 0
    ? args.ToList()
    : [Directory.GetCurrentDirectory()];

try
{
    // Create and run an MCP server that provides filesystem access
    await new McpServerBuilder()
        .WithServerInfo("FileSystemServer", "1.0.0", "MCP server providing filesystem access")
        // Use standard input/output for communication
        .UseStdio()
        // Add filesystem tools with security restrictions
        .AddFileSystemTools(allowedDirectories)
        .BuildAndRunAsync();
}
catch (Exception ex)
{
    // Log errors to stderr (stdout is reserved for MCP protocol)
    Console.Error.WriteLine($"[ERROR] Server failed: {ex.Message}");
    Console.Error.WriteLine($"[ERROR] Stack: {ex.StackTrace}");
    Environment.Exit(1);
}
