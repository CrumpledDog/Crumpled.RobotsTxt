# Copilot / Agent Instructions — Crumpled.RobotsTxt

A configuration-driven robots.txt package for Umbraco 13 to 18, with Content Signals support. See
[README.md](../README.md) for the overview and [CONTRIBUTING.md](../CONTRIBUTING.md) for the branching strategy.

## Repo shape

```
src/
  Crumpled.RobotsTxt/            - the packable Umbraco package (DynamicRobotsTxtProvider renders robots.txt)
  Crumpled.RobotsTxt.Core/       - fluent-builder robots.txt middleware
  Crumpled.RobotsTxt.TestSite*/  - dev sites for Umbraco 17, 13 and 18
tests/
  Crumpled.RobotsTxt.Tests.Integration/      - Umbraco 13 (net8.0) and 17 (net10.0)
  Crumpled.RobotsTxt.Tests.Integration.V18/  - Umbraco 18
```

## Output ordering

Within each `User-agent` group: group-level `Content-Signal` first, then `Crawl-delay`, `Disallow`, path-specific
`Content-Signal` + `Allow` pairs, then plain `Allow`. Specific agents come first, `*` last.

## Formatting

`dotnet format --verify-no-changes` must pass in CI. Run `dotnet format` before committing.

## Versioning

No hardcoded versions for the package — semantic-release computes the version from Conventional Commits
and injects it at pack time via `/p:PackageVersion=`. Don't hand-edit version numbers.
