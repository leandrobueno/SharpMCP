# SharpMCP Templates

This package contains project and item templates for SharpMCP, a .NET framework for building Model Context Protocol (MCP) servers.

## Installation

```bash
dotnet new install SharpMCP.Templates
```

## Templates

### Project Templates

- **mcpserver** - Basic MCP server with one example tool
- **mcptoolset** - MCP server with multiple related tools

### Item Templates

- **mcptool** - Individual MCP tool implementation

## Usage

### Create a new MCP server
```bash
dotnet new mcpserver -n MyServer
```

### Add a new tool to existing project
```bash
dotnet new mcptool -n MyTool
```

### Create a server with multiple tools
```bash
dotnet new mcptoolset -n MyToolCollection
```

## Template Options

### mcpserver
- `--framework` - Target framework (default: net9.0, options: net8.0, net9.0)
- `--useDI` - Include dependency injection setup (default: false)

### mcptool
- `--async` - Make tool async (default: true)
- `--namespace` - Namespace for the tool (default: MyNamespace)
- `--description` - Description of what the tool does (default: "Performs a specific operation")
- `--toolId` - The ID used to identify the tool (default: my_tool)

### mcptoolset
- `--framework` - Target framework (default: net9.0, options: net8.0, net9.0)
- `--toolCount` - Number of example tools to create (default: 3, options: 2, 3, 5)

For more information, visit [SharpMCP Documentation](https://github.com/leandrobueno/SharpMCP)
