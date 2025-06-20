# SharpMCP Roadmap

## Overview

SharpMCP is a framework for building Model Context Protocol (MCP) servers in C#/.NET. This roadmap outlines the planned features and development milestones.

**Framework Version**: .NET 9.0  
**Last Updated**: January 2025

## Status Legend

- 🔴 Not Started
- 🟡 In Progress
- 🟢 Completed
- 🔵 In Review
- ⚪ Planned (future)

## Version 1.0.0 - Core Foundation

### Completed in SharpMCP.Core
- ✅ All core interfaces and abstractions
- ✅ Protocol layer (JSON-RPC and MCP types)
- ✅ Tool system interfaces (`IMcpTool`, attributes)
- ✅ Transport abstraction (`IMcpTransport`)
- ✅ Server interfaces (`IMcpServer`, `IMcpServerBuilder`)
- ✅ Schema attributes and model classes
- ✅ Utility classes (`ToolResponseBuilder`)
- ✅ Basic examples and unit tests

### Completed for v1.0.0

- 🟢 **Base Server Implementation** ✓
- 🟢 **Protocol Abstractions** ✓
- 🟢 **Tool System** ✓
- 🟢 **Tool Base Classes** ✓
- 🟢 **Common Tool Implementations** ✓
- 🟢 **JSON Schema Support** ✓
- 🟢 **Server Configuration** ✓

### Remaining for v1.0.0

### Developer Experience
- 🔴 **Project Templates**
- 🔴 **Documentation**

### Testing Support
- 🟡 **Testing Utilities**

## Version 1.1.0 - Enhanced Features

### Middleware System
- ⚪ **Middleware Pipeline**
  - `IMcpMiddleware` interface
  - Built-in middleware components:
    - Request validation
    - Response caching
    - Rate limiting
    - Metrics collection

### Advanced Tool Features
- ⚪ **Batch Processing Support**
  - `BatchProcessingToolBase`
  - Parallel execution
  - Progress reporting
  - Cancellation support

- ⚪ **Streaming Tools**
  - Streaming response support
  - Incremental result delivery
  - Progress notifications

### Dependency Injection
- ⚪ **DI Container Integration**
  - Microsoft.Extensions.DependencyInjection support
  - Service registration extensions
  - Scoped tool instances
  - Configuration injection

## Version 1.2.0 - Extended Transports

### Additional Transports
- ⚪ **HTTP Transport**
  - REST endpoint hosting
  - WebSocket support
  - Authentication middleware

- ⚪ **Named Pipe Transport**
  - Windows/Unix named pipes
  - Local IPC scenarios

### Resource System
- ⚪ **Resource Provider Support**
  - Resource registration
  - Dynamic resource discovery
  - Resource change notifications

## Version 2.0.0 - Enterprise Features

### Advanced Features
- ⚪ **Distributed Scenarios**
  - Server clustering
  - Load balancing
  - State synchronization

- ⚪ **Monitoring & Observability**
  - OpenTelemetry integration
  - Structured logging enhancements
  - Performance metrics
  - Distributed tracing

- ⚪ **Security Enhancements**
  - Authentication providers
  - Authorization policies
  - Audit logging
  - Encryption support

## Future Considerations

### Tooling
- ⚪ Visual Studio extension
- ⚪ CLI tool for server management
- ⚪ Server explorer/debugger

### Ecosystem
- ⚪ Plugin system
- ⚪ Community tool repository
- ⚪ Integration packages (Entity Framework, Dapper, etc.)

## Contributing

We welcome contributions! Priority areas for community involvement:

1. Testing and bug reports
2. Documentation improvements
3. Example implementations
4. Tool implementations
5. Transport implementations

## Release Schedule

- **v1.0.0** - Target: Q1 2025 (Ready for release)
- **v1.1.0** - Target: Q3 2025
- **v1.2.0** - Target: Q4 2025
- **v2.0.0** - Target: Q2 2026

---

Last Updated: January 2025