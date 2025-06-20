# SharpMCP.Core

Core abstractions and interfaces for the SharpMCP framework.

## Overview

This library contains the fundamental types and interfaces that define the MCP protocol implementation:

- **Protocol** - JSON-RPC message types and MCP protocol definitions
- **Tools** - Tool interfaces and base classes for creating MCP tools
- **Schema** - JSON Schema generation attributes and types
- **Transport** - Transport abstraction for different communication mechanisms
- **Server** - Server interfaces and configuration
- **Utils** - Helper classes and utilities

## Key Types

### Protocol
- `JsonRpcMessage`, `JsonRpcRequest`, `JsonRpcResponse` - Core JSON-RPC types
- `ServerInfo`, `Tool`, `ToolResponse` - MCP protocol types

### Tools
- `IMcpTool` - Interface for implementing tools
- `McpToolAttribute` - Attribute for marking tool classes
- `McpToolException` - Exception type for tool errors

### Schema
- `JsonSchemaAttribute`, `JsonRequiredAttribute` - Schema generation attributes
- `JsonSchema` - JSON Schema representation

### Transport
- `IMcpTransport` - Transport abstraction interface

### Server
- `IMcpServer` - Server interface
- `IMcpServerBuilder` - Fluent builder for creating servers
- `McpServerOptions` - Server configuration options

## Usage

This library is typically not used directly. Instead, use `SharpMCP.Server` which provides concrete implementations of these abstractions.

## NuGet Package

```
Install-Package SharpMCP.Core
```
