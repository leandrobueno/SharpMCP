# Contributing to SharpMCP

Thank you for your interest in contributing to SharpMCP! We welcome contributions from the community and are grateful for any help you can provide.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct. Please be respectful and considerate in all interactions.

## How to Contribute

### Reporting Issues

- Check if the issue already exists in the [issue tracker](https://github.com/leandrobueno/SharpMCP/issues)
- Use the appropriate issue template
- Provide as much detail as possible, including:
  - Steps to reproduce
  - Expected behavior
  - Actual behavior
  - Environment details (.NET version, OS, etc.)

### Suggesting Features

- Open a discussion in [GitHub Discussions](https://github.com/leandrobueno/SharpMCP/discussions)
- Describe the feature and its use cases
- Explain why it would be valuable to the project

### Submitting Pull Requests

1. **Fork the repository**
   ```bash
   git clone https://github.com/leandrobueno/SharpMCP.git
   cd SharpMCP
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow the coding standards (see below)
   - Add tests for new functionality
   - Update documentation as needed

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Commit your changes**
   ```bash
   git commit -m "Add feature: your feature description"
   ```

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**
   - Provide a clear description of the changes
   - Reference any related issues
   - Ensure all CI checks pass

## Development Guidelines

### Coding Standards

- Follow standard C# naming conventions
- Use meaningful variable and method names
- Keep methods focused and small
- Add XML documentation comments to public APIs
- Use async/await for asynchronous operations

### Code Style

We use the default .NET code style. Key points:
- 4 spaces for indentation (no tabs)
- Opening braces on new lines
- Use `var` when the type is obvious
- Prefer expression-bodied members when appropriate

Example:
```csharp
/// <summary>
/// Executes the tool with the specified arguments.
/// </summary>
public async Task<ToolResponse> ExecuteAsync(ToolArgs args, CancellationToken cancellationToken)
{
    if (args == null)
    {
        throw new ArgumentNullException(nameof(args));
    }

    var result = await ProcessAsync(args, cancellationToken);
    return new ToolResponse { Content = result };
}
```

### Testing

- Write unit tests for all new functionality
- Aim for high code coverage (>80%)
- Use descriptive test names that explain what is being tested
- Follow the Arrange-Act-Assert pattern

Example:
```csharp
[Fact]
public async Task ExecuteAsync_WithValidArgs_ReturnsExpectedResponse()
{
    // Arrange
    var tool = new MyTool();
    var args = new ToolArgs { Input = "test" };

    // Act
    var response = await tool.ExecuteAsync(args, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
    response.Content.Should().Be("Expected output");
}
```

### Documentation

- Update README.md if adding major features
- Add XML documentation to all public APIs
- Include examples in documentation when helpful
- Update ROADMAP.md if implementing planned features

## Building the Project

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code (optional)

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack -c Release
```

## Release Process

1. Update version numbers in `Directory.Build.props`
2. Update CHANGELOG.md
3. Create a release PR
4. After merge, tag the release
5. CI will automatically publish to NuGet

## Getting Help

- Join our [Discord server](https://discord.gg/sharpmcp) (Coming Soon)
- Ask questions in [GitHub Discussions](https://github.com/leandrobueno/SharpMCP/discussions)
- Check the [documentation](docs/)

## Recognition

Contributors will be recognized in:
- The project README
- Release notes
- Our contributors page

Thank you for helping make SharpMCP better!
