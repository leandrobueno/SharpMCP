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
- `--framework` - Target framework (default: net9.0)
- `--use-di` - Include dependency injection setup (default: false)

### mcptool
- `--async` - Make tool async (default: true)
- `--namespace` - Namespace for the tool (default: current namespace)

### mcptoolset
- `--framework` - Target framework (default: net9.0)
- `--tool-count` - Number of example tools to create (default: 3)

For more information, visit [SharpMCP Documentation](https://github.com/leandrobueno/SharpMCP)
