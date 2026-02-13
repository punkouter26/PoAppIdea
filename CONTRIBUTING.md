# Contributing to PoAppIdea

Thank you for your interest in contributing to PoAppIdea!

## How to Contribute

### Reporting Bugs
1. Check if the bug has already been reported
2. Create a detailed issue with:
   - Clear title
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details

### Suggesting Features
1. Open an issue with the `enhancement` label
2. Describe the feature and its use case
3. Provide any relevant mockups or examples

### Pull Requests
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes following our coding standards
4. Write tests for new features
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

### Coding Standards

- Follow [SOLID principles](https://en.wikipedia.org/wiki/SOLID)
- Use Vertical Slice Architecture for features
- Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- Add tests for new features
- Use meaningful variable and method names
- Keep methods small and focused

### Project Structure

```
PoAppIdea/
├── src/
│   ├── PoAppIdea.Web/       # Blazor Server app
│   │   ├── Components/       # Pages and components
│   │   ├── Features/         # Vertical slice modules
│   │   └── Infrastructure/   # AI, Storage, Auth
│   ├── PoAppIdea.Core/       # Domain entities
│   └── PoAppIdea.Shared/     # DTOs and contracts
└── tests/
    ├── PoAppIdea.UnitTests/
    ├── PoAppIdea.IntegrationTests/
    └── PoAppIdea.E2E/
```

## Getting Started

See [LocalSetup.md](docs/LocalSetup.md) for development environment setup.

## Questions?

Feel free to open an issue for questions about contributing.
