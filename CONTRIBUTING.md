# Contributing to Haven

Thank you for your interest in contributing to Haven! We welcome contributions from everyone.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a new branch for your feature or fix
4. Make your changes
5. Test your changes thoroughly
6. Commit your changes with clear, descriptive messages
7. Push to your fork
8. Submit a pull request

## Development Setup

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+ (for web client development)
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Flick851/Haven.git
cd Haven

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Run the server
dotnet run --project src/Haven.Server
```

## Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Comment your code where necessary
- Keep methods small and focused
- Write unit tests for new functionality

## Pull Request Process

1. Ensure your code builds without warnings
2. Include tests for new functionality
3. Update documentation as needed
4. Ensure all tests pass
5. Update the README.md if needed
6. Reference any related issues

## Feature Development

When developing new features:

1. Discuss major changes in an issue first
2. Keep Haven's dual-branding system intact
3. Maintain API compatibility with Jellyfin
4. Ensure features can be toggled on/off
5. Document new API endpoints

## Reporting Issues

- Use the GitHub issue tracker
- Include steps to reproduce
- Provide system information
- Include relevant logs
- Be respectful and constructive

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers
- Focus on what's best for the community
- Show empathy towards others

## License

By contributing to Haven, you agree that your contributions will be licensed under the GPL-2.0 License.

## Questions?

Join our community on Discord or open a discussion on GitHub.

Thank you for contributing to Haven!