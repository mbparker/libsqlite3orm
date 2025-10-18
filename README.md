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

### Setup Requirements

To enable automatic NuGet publishing, configure Trusted Publishing on NuGet.org:

1. Go to https://www.nuget.org and sign in
2. Navigate to your package or account settings
3. Go to "Trusted Publishers" section
4. Add a new trusted publisher with:
   - **Package Owner**: Your NuGet.org username or organization
   - **Repository Owner**: mbparker
   - **Repository Name**: libsqlite3orm
   - **Workflow Name**: build-test-publish.yml
   - **Environment**: (leave empty)

This uses OpenID Connect (OIDC) for secure authentication without requiring API keys.

### Triggering a Release

To publish a new version to NuGet:

1. Update the version number in `LibSqlite3Orm/LibSqlite3Orm.csproj`
2. Commit and push your changes
3. Create a new release on GitHub with a tag (e.g., `v1.2.55`)
4. The workflow will automatically build, test, and publish to NuGet

