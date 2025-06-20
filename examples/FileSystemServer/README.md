# FileSystemServer - MCP Implementation

## Overview
This is an example MCP (Model Context Protocol) server that provides filesystem access capabilities.

## The JSON-RPC stdout Issue

### Problem
The JavaScript MCP client (like Claude AI) was receiving a Zod validation error because non-JSON content was being written to stdout, breaking the JSON-RPC protocol.

### Solution
The fix involves using a C# Module Initializer to redirect stdout to null before ANY code executes:

```csharp
[ModuleInitializer]
public static void Initialize()
{
    // Redirect stdout immediately when the module loads
    Console.SetOut(TextWriter.Null);
}
```

This ensures that:
- No .NET runtime messages go to stdout
- No assembly loading messages go to stdout  
- Only valid JSON-RPC messages are sent to stdout
- All logging goes to stderr (MCP-safe)

## Building

```bash
# Build in Release mode
build-release.bat

# Or manually:
dotnet build --configuration Release
```

## Testing

To verify the fix works correctly:

```bash
# Run the verification test
verify-fix.bat
```

This will:
1. Build the server
2. Send an initialize request
3. Verify stdout contains only valid JSON
4. Show stderr logs

## Running the Server

```bash
# From the bin\Release\net8.0 directory:
FileSystemServer.exe "C:\allowed\directory" "D:\another\allowed\directory"

# Or with no arguments (uses current directory):
FileSystemServer.exe
```

## Key Files

- `Program.cs` - Main server implementation with Module Initializer fix
- `verify-fix.bat` - Test script to verify the JSON-RPC fix works
- `build-release.bat` - Build script for Release configuration

## Notes

- Always use Release builds for production
- The server uses stdin/stdout for JSON-RPC communication
- All logging goes to stderr to avoid corrupting the JSON-RPC stream
- The Module Initializer ensures stdout is redirected before any code runs
