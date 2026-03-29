# CI/CD Pipeline

Windows Security Manager uses two GitHub Actions workflows to automate quality checks, builds, and releases.

## Overview

```
Push / PR to main ──► ci.yml ──► Format check ──► Build ──► Test + Coverage
                                                                   │
                                                           Upload coverage artifact

Push tag v* ────────► release.yml ──► Test ──► Publish EXE ──► GitHub Release
workflow_dispatch ──►                              │
                                          Upload build artifact
```

---

## CI Workflow (`ci.yml`)

**File:** [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)

### Triggers

| Event | Condition |
|---|---|
| `push` | Commits pushed to `main` |
| `pull_request` | PRs targeting `main` |

### Steps

| # | Step | Description |
|---|---|---|
| 1 | **Checkout** | Fetches the repository code |
| 2 | **Setup .NET** | Installs .NET 8.0 SDK |
| 3 | **Cache NuGet** | Restores cached NuGet packages for faster builds |
| 4 | **Restore** | Runs `dotnet restore` to download any missing dependencies |
| 5 | **Format check** | Runs `dotnet format --verify-no-changes` to enforce consistent code style |
| 6 | **Build** | Compiles the solution in Release configuration |
| 7 | **Test + Coverage** | Runs all xUnit tests and collects code coverage using Coverlet |
| 8 | **Upload coverage** | Uploads Cobertura XML coverage report as a workflow artifact |

### What It Enforces

- **Code formatting** — Any formatting violation fails the build. Run `dotnet format` locally to fix issues before pushing.
- **Compilation** — The project must compile cleanly in Release mode.
- **All tests pass** — Every xUnit test must pass.
- **Coverage tracking** — Coverage reports are available as downloadable artifacts on each workflow run.

### Running Locally

You can replicate the CI checks locally:

```bash
# Format check (fix issues with: dotnet format)
dotnet format --verify-no-changes

# Build
dotnet build --configuration Release

# Test with coverage
dotnet test tests/WindowsSecurityManager.Tests/WindowsSecurityManager.Tests.csproj \
    --configuration Release \
    --collect:"XPlat Code Coverage"
```

---

## Release Workflow (`release.yml`)

**File:** [`.github/workflows/release.yml`](../.github/workflows/release.yml)

### Triggers

| Event | Condition |
|---|---|
| `push` (tags) | Tag matching `v*` (e.g., `v1.0.0`, `v2.1.0-beta`) |
| `workflow_dispatch` | Manual trigger from the Actions tab |

### Steps

| # | Step | Description |
|---|---|---|
| 1 | **Checkout** | Fetches the repository code |
| 2 | **Setup .NET** | Installs .NET 8.0 SDK |
| 3 | **Cache NuGet** | Restores cached NuGet packages for faster builds |
| 4 | **Restore** | Runs `dotnet restore` |
| 5 | **Test** | Runs all tests in Release configuration |
| 6 | **Publish** | Creates a self-contained, single-file `win-x64` executable |
| 7 | **Upload artifact** | Uploads `WindowsSecurityManager.exe` as a workflow artifact |
| 8 | **Create release** | *(Tag push only)* Creates a GitHub Release with auto-generated release notes and the `.exe` attached |

### Publishing Configuration

The executable is published with these settings (from the `.csproj`):

| Setting | Value | Purpose |
|---|---|---|
| `PublishSingleFile` | `true` | Bundles everything into one `.exe` |
| `SelfContained` | `true` | Includes the .NET runtime — no install needed |
| `EnableCompressionInSingleFile` | `true` | Compresses the binary for a smaller download |
| `IncludeNativeLibrariesForSelfExtract` | `true` | Ensures native libs are extractable at runtime |
| Runtime | `win-x64` | Targets 64-bit Windows |

### Creating a Release

To create a new release:

```bash
# Tag the commit
git tag v1.2.0

# Push the tag to trigger the release workflow
git push origin v1.2.0
```

The workflow will automatically:
1. Run all tests
2. Build the standalone executable
3. Create a GitHub Release with the tag name
4. Attach `WindowsSecurityManager.exe` to the release
5. Generate release notes from commit history

---

## NuGet Caching

Both workflows cache NuGet packages using `actions/cache@v4`:

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

The cache key is based on the hash of all `.csproj` files, so it automatically invalidates when dependencies change.

---

## Code Coverage

Coverage is collected using [Coverlet](https://github.com/coverlet-coverage/coverlet) during CI test runs. The output is a Cobertura XML report uploaded as a workflow artifact.

**To download coverage reports:**
1. Go to the [Actions tab](https://github.com/Kvikku/Windows-Security-Manager/actions/workflows/ci.yml)
2. Click on a workflow run
3. Download the `coverage-report` artifact

**To generate coverage locally:**

```bash
dotnet test tests/WindowsSecurityManager.Tests/WindowsSecurityManager.Tests.csproj \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults
```

The Cobertura XML report will be in `./TestResults/<guid>/coverage.cobertura.xml`.

---

## Permissions

| Workflow | Permission | Reason |
|---|---|---|
| `ci.yml` | `contents: read` | Read-only access to check out code |
| `release.yml` | `contents: write` | Required to create GitHub Releases |
