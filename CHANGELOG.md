# Changelog

All notable changes to SharpMCP will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- SharpMCP.Tools.Common implementation:
  - **File System Tools**
    - Complete set of 10 file system tools adapted from NETFileSystem
    - Security with path validation and allowed directories
    - Parallel file operations with configurable concurrency
    - Support for simple, wildcard, and regex search patterns
    - Extension methods for easy integration with MCP servers
  - **Tool Features**
    - Read single and multiple files
    - Write files and create directories
    - Move/rename operations
    - Directory listing and tree visualization
    - File search with pattern matching
    - File metadata retrieval
  - **Integration**
    - `CommonTools.RegisterFileSystemTools()` for bulk registration
    - `AddFileSystemTools()` extension method for builders
    - Configurable allowed directories for security
- Complete SharpMCP.Server implementation:
  - **Server Core**
    - `McpServerBase` abstract class with complete JSON-RPC message handling
    - Full implementation of `IMcpServer` interface with all required members
    - Server lifecycle management (Started/Stopped events)
    - Tool execution events with timing and error tracking
    - Concurrent tool registry using `ConcurrentDictionary`
    - Dynamic capability updates based on registered features
  - **Transport Layer**
    - `StdioTransport` for stdin/stdout communication
    - Thread-safe message sending with semaphore locking
    - Proper connection state management
    - Comprehensive error handling for IO operations
  - **Tool System**
    - `McpToolBase<TArgs>` generic base class with automatic argument parsing
    - `McpToolBase` for parameter-less tools
    - Built-in argument validation support
    - Automatic `GetInputSchema()` implementation
    - Fluent response building helpers
  - **Schema Generation**
    - `JsonSchemaGenerator` for automatic schema generation from C# types
    - Support for all JSON Schema constraints (string, number, array)
    - Attribute-based schema customization
    - Proper handling of nullable types and enums
    - JSON property name mapping support
  - **Builder Pattern**
    - `McpServerBuilder` fluent configuration API
    - `DefaultMcpServer` internal implementation
    - Support for custom server implementations
    - Automatic tool discovery from assemblies
    - `BuildAndRunAsync` for quick server startup
- Initial project structure and organization
- SharpMCP.Core library with all core abstractions:
  - **Protocol Layer**
    - JSON-RPC message types (`JsonRpcRequest`, `JsonRpcResponse`, `JsonRpcError`)
    - MCP protocol types (`ServerInfo`, `Tool`, `ToolResponse`, etc.)
    - Standard JSON-RPC error codes
  - **Tool System**
    - `IMcpTool` interface for tool implementations
    - `McpToolAttribute` for declarative tool registration
    - `McpToolException` for tool-specific errors
    - Tool execution context support
  - **Schema Support**
    - Comprehensive attribute system for JSON Schema generation
    - `JsonSchemaAttribute`, `JsonRequiredAttribute`, `JsonDescriptionAttribute`
    - Constraint attributes for strings, numbers, and arrays
    - `JsonEnumAttribute` for enumeration constraints
    - `JsonSchema` model class
  - **Transport Abstraction**
    - `IMcpTransport` interface for pluggable transports
    - `McpTransportBase` abstract base class
    - `McpTransportException` for transport errors
  - **Server Interfaces**
    - `IMcpServer` core server interface
    - `IMcpServerBuilder` fluent builder interface
    - `McpServerOptions` configuration class
    - Server lifecycle event arguments
  - **Utilities**
    - `ToolResponseBuilder` for fluent response construction
    - `McpJsonContext` for high-performance JSON serialization
    - Global using directives for common namespaces
- Comprehensive unit tests for core components
- Example implementations demonstrating usage patterns
- Project documentation (README, ROADMAP, CONTRIBUTING)
- Build scripts for cross-platform development
- EditorConfig for consistent code style

### Changed
- Updated to .NET 9.0 framework
- Fixed compilation issues in attribute system
- Improved property naming to avoid conflicts

### Technical Details
- Target Framework: .NET 9.0
- Language Version: C# 13
- Nullable Reference Types: Enabled
- JSON Serialization: System.Text.Json with source generation

## [0.0.0] - Project Initialized

- Initial repository setup
- Basic project structure defined
