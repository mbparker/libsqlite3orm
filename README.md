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

To enable automatic NuGet publishing, add a repository secret named `NUGET_API_KEY` containing your NuGet API key:

1. Go to your repository Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `NUGET_API_KEY`
4. Value: Your NuGet API key from https://www.nuget.org/account/apikeys
5. Click "Add secret"

### Triggering a Release

To publish a new version to NuGet:

1. Update the version number in `LibSqlite3Orm/LibSqlite3Orm.csproj`
2. Commit and push your changes
3. Create a new release on GitHub with a tag (e.g., `v1.2.55`)
4. The workflow will automatically build, test, and publish to NuGet

