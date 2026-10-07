# Contributing

## Formatting

```bash
# Format all C# code (REQUIRED before committing)
dotnet format

# Verify formatting (used in CI)
dotnet format --verify-no-changes
```

All PRs must pass `dotnet format --verify-no-changes` in CI or they will be rejected.

### Anti-Patterns to Avoid

- Don't commit code that fails `dotnet format`
- Don't use real credentials or organization names in examples/docs
- Don't add features without updating documentation
- Don't use tabs for indentation
- Don't ignore logger warnings

## Branching Strategy

This project uses Git Flow with version-specific branches. The current line is `v3`, which supports Umbraco 13 to 18.

### Main Branches

- **`develop/v3`** - Main development branch (protected, default branch)
- **`release/v3`** - Stable release branch for production releases (protected)
- **`beta/v3`** - Beta releases (protected, created only once actually needed)

### Supporting Branches

- **`feature/v3/<feature-name>`** - New features
  - Branch from: `develop/v3`
  - Merge back into: `develop/v3`
  - Example: `feature/v3/add-thing`

- **`hotfix/v3/<fix-name>`** - Urgent production fixes
  - Branch from: `release/v3`
  - Merge back into: `release/v3` and `develop/v3`
  - Example: `hotfix/v3/fix-null-reference`

### Branching Workflow

```bash
# Create a feature branch
git checkout develop/v3
git pull origin develop/v3
git checkout -b feature/v3/my-new-feature

# Work on your feature, commit regularly
git add .
git commit -m "feat: add new feature"

# Push your branch
git push -u origin feature/v3/my-new-feature

# Create a pull request to develop/v3
```

## Testing

Integration tests run against several Umbraco majors:

```bash
dotnet test tests/Crumpled.RobotsTxt.Tests.Integration --framework net8.0    # Umbraco 13
dotnet test tests/Crumpled.RobotsTxt.Tests.Integration --framework net10.0   # Umbraco 17
dotnet test tests/Crumpled.RobotsTxt.Tests.Integration.V18                   # Umbraco 18
```

## Commit Messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/) for automated versioning and
changelog generation via semantic-release.

Format: `<type>[optional scope]: <description>`

Common types: `feat` (minor bump), `fix` (patch bump), `chore`/`refactor`/`style`/`test`/`build`/`ci`/`docs`/`perf`/`deps`
(patch bump). A `BREAKING CHANGE:` footer (or `!` after the type) triggers a major bump.
