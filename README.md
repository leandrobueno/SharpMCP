# SharpMCP

<div align="center">

[![NuGet Version](https://img.shields.io/nuget/v/SharpMCP.Server)](https://www.nuget.org/packages/SharpMCP.Server)
[![Build Status](https://img.shields.io/github/actions/workflow/status/leandrobueno/SharpMCP/build.yml?branch=main)](https://github.com/leandrobueno/SharpMCP/actions)
[![License](https://img.shields.io/github/license/leandrobueno/SharpMCP)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-available-brightgreen)](https://leandrobueno.github.io/SharpMCP)

**A modern .NET framework for building Model Context Protocol (MCP) servers**

[Getting Started](#getting-started) • [Documentation](docs/) • [Examples](examples/) • [Contributing](#contributing)

</div>

## Overview

SharpMCP is a comprehensive framework that simplifies the creation of MCP servers in C#/.NET. It provides a robust foundation with abstractions, utilities, and common implementations that allow developers to focus on their specific use cases rather than protocol implementation details.

### What is MCP?

The Model Context Protocol (MCP) is an open protocol that enables secure, controlled interactions between AI applications and external data sources or tools. MCP servers act as bridges between AI models and your systems, providing structured access to information and capabilities.

## Features

- 🚀 **Easy to Use** - Simple, intuitive APIs with sensible defaults
- 🛠️ **Flexible Architecture** - Extensible design supporting custom tools and transports
- 📦 **Batteries Included** - Common tool implementations and patterns out of the box
- 🧪 **Testing Support** - Built-in testing harness and utilities
- 📚 **Well Documented** - Comprehensive documentation and examples
- ⚡ **High Performance** - Optimized for efficiency with async/await throughout
- 🔒 **Secure by Default** - Built-in security features and best practices

## Getting Started

### Prerequisites

- .NET 9.0 or later
- Visual Studio 2022, VS Code, or your preferred IDE

### Quick Start

1. **Install SharpMCP.Server (includes everything you need)**

```bash
# Install the complete server package
dotnet add package SharpMCP.Server
```

This single package automatically includes:
- **SharpMCP.Core** - Core abstractions, interfaces, and protocol support
- **SharpMCP.Tools.Common** - File system tools, archive operations, and utilities

2. **Create a simple MCP server**

```csharp
using SharpMCP.Core.Tools;
using SharpMCP.Core.Schema;
using SharpMCP.Server;
using SharpMCP.Tools.Common;

// Define a simple tool
[McpTool("hello", Description = "Says hello to someone")]
public class HelloTool : McpToolBase<HelloArgs>
{
    public override string Name => "hello";
    public override string? Description => "Says hello to someone";

    protected override Task<ToolResponse> ExecuteAsync(HelloArgs args, CancellationToken ct)
    {
        return Task.FromResult(Success($"Hello, {args.Name}!"));
    }
}

public class HelloArgs
{
    [JsonRequired]
    [JsonDescription("Name of the person to greet")]
    public string Name { get; set; } = "";
}

// Create and run the server
class Program
{
    static async Task Main(string[] args)
    {
        var server = new McpServerBuilder()
            .WithName("HelloServer")
            .WithVersion("1.0.0")
            .AddTool(new HelloTool())
            .AddFileSystemTools() // Add common file system tools
            .Build();

        await server.RunAsync();
    }
}
```

3. **Run your server**

```bash
dotnet run
```

### Using Built-in Tools

SharpMCP.Server includes a comprehensive set of file system tools out of the box:

```csharp
var server = new McpServerBuilder()
    .WithName("FileServer")
    .WithVersion("1.0.0")
    .AddFileSystemTools(allowedDirectories: ["/safe/directory/path"])
    .Build();
```

Available tools include:
- File operations (read, write, create, move)
- Directory operations (list, create, tree view)
- Search capabilities (pattern matching, regex)
- Archive operations (zip, extract, list contents)
- Security utilities with path validation

### Package Structure

The SharpMCP framework consists of three packages with a clean dependency hierarchy:

```
SharpMCP.Server (main package)
├── SharpMCP.Core (automatically included)
│   ├── Core abstractions and interfaces
│   ├── Protocol implementation
│   ├── Tool base classes
│   └── JSON schema generation
└── SharpMCP.Tools.Common (automatically included)
    ├── File system tools
    ├── Archive operations
    ├── Security utilities
    └── Common patterns
```

**Installation:** Only install `SharpMCP.Server` - it automatically includes Core and Tools.Common.

### Using Project Templates

For the fastest start, use our project templates:

```bash
# Install templates
dotnet new install SharpMCP.Templates

# Create a new MCP server project
dotnet new mcpserver -n MyAwesomeServer

# Create a new tool
cd MyAwesomeServer
dotnet new mcptool -n MyCustomTool
```

## Architecture

SharpMCP follows a modular architecture:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Your Tools    │     │   Your Server   │     │    Transport    │
│                 │────▶│                 │────▶│    (stdio)      │
│ - Custom Logic  │     │ - Configuration │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │                        │
         ▼                       ▼                        ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│ SharpMCP.Tools  │     │ SharpMCP.Server │     │  SharpMCP.Core  │
│                 │     │                 │     │                 │
│ - Base Classes  │     │ - Server Base   │     │ - Interfaces    │
│ - Common Tools  │     │ - DI Support    │     │ - Protocol      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

## Examples

Check out the [examples](examples/) directory for complete working examples:

- **[FileSystemServer](examples/FileSystemServer)** - Secure file system access
- **[DatabaseServer](examples/DatabaseServer)** - SQL database operations (Coming Soon)
- **[ApiGatewayServer](examples/ApiGatewayServer)** - REST API integration (Coming Soon)

## Documentation

- [Getting Started Guide](docs/getting-started.md)
- [Creating Tools](docs/creating-tools.md)
- [Configuration](docs/configuration.md)
- [Testing](docs/testing.md)
- [Security Best Practices](docs/security.md)
- [API Reference](docs/api/)

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/leandrobueno/SharpMCP.git
cd SharpMCP

# Build the solution
dotnet build

# Run tests
dotnet test

# Pack NuGet packages locally
dotnet pack -o ./artifacts
```

### Areas for Contribution

- 🐛 Bug fixes and improvements
- 📝 Documentation enhancements
- 🧪 Additional test coverage
- 🛠️ New tool implementations
- 🚀 Performance optimizations
- 💡 Feature suggestions

## Roadmap

See our [detailed roadmap](ROADMAP.md) for planned features and milestones.

### Upcoming Features

- ✅ Core server infrastructure (v1.0)
- 🔄 Middleware pipeline system (v1.1)
- 🔄 Additional transport protocols (v1.2)
- 📅 Advanced monitoring and telemetry (v2.0)

## Community

- **GitHub Discussions** - Ask questions and share ideas
- **Issues** - Report bugs or request features
- **Discord** - Join our community chat (Coming Soon)

## License

SharpMCP is licensed under the [MIT License](LICENSE).

## Development Status

### Completed Components
- ✅ Core abstractions and interfaces (SharpMCP.Core)
- ✅ Protocol layer with JSON-RPC support
- ✅ Tool system interfaces and attributes
- ✅ JSON Schema attribute system
- ✅ Transport abstractions
- ✅ Server interfaces and builder pattern
- ✅ Utility classes and response builders
- ✅ Common file system tools (SharpMCP.Tools.Common)
- ✅ Archive operations and security utilities

### In Progress
- 🔄 Server implementation (SharpMCP.Server)
- 🔄 StdioTransport implementation
- 🔄 Enhanced testing framework

## Acknowledgments

- The [Model Context Protocol](https://github.com/modelcontextprotocol) team for creating MCP
- The .NET community for continuous support and feedback
- All our contributors and users

---
