# libsqlite3orm
A cross platform, easy to use, Object Relational Mapper for SQLite 3

## Continuous Integration and Deployment

This project uses GitHub Actions for automated building, testing, and publishing to NuGet.

### Workflow Features

- **Multi-platform Testing**: The solution is automatically built and tested on:
  - Linux (ubuntu-latest)
  - Windows (windows-latest)
  - macOS (macos-latest)

- **Automated Testing**: All tests are run on every push and pull request to ensure code quality

- **NuGet Publishing**: When a new release is published on GitHub, the NuGet package is automatically:
  - Built in Release configuration
  - Packed
  - Published to NuGet.org

