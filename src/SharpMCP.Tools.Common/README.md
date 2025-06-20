# SharpMCP.Tools.Common

Common tool implementations for SharpMCP servers.

## Features

### File System Tools
- **ReadFileTool** - Read file contents
- **ReadMultipleFilesTool** - Read multiple files in parallel
- **WriteFileTool** - Create or overwrite files
- **CreateDirectoryTool** - Create directories
- **MoveFileTool** - Move or rename files/directories
- **ListDirectoryTool** - List directory contents
- **DirectoryTreeTool** - Get recursive directory structure
- **SearchFilesTool** - Search files with patterns (simple, wildcard, regex)
- **GetFileInfoTool** - Get file metadata
- **ListAllowedDirectoriesTool** - List accessible directories

## Usage

```csharp
using SharpMCP.Server;
using SharpMCP.Tools.Common;

// Option 1: Use extension method
var server = new McpServerBuilder()
    .WithServerInfo("MyServer", "1.0.0")
    .AddFileSystemTools(allowedDirectories: new List<string> { @"C:\MyProject" })
    .Build();

// Option 2: Register manually
var allowedDirs = new List<string> { @"C:\MyProject", @"D:\Data" };
CommonTools.RegisterFileSystemTools(server, allowedDirs);
```

## Security

All file system operations are restricted to allowed directories. Attempts to access files outside these directories will throw `UnauthorizedAccessException`.

## Dependencies

- SharpMCP.Core
- SharpMCP.Server
