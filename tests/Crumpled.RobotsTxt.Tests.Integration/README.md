# Crumpled.RobotsTxt Integration Tests

Integration tests for the Crumpled.RobotsTxt package using WebApplicationFactory with xUnit.

## Overview

These tests spin up a full Umbraco instance using the TestSite and validate robots.txt responses using different configurations.

## Technology Stack

- **xUnit** - Test framework
- **WebApplicationFactory** - ASP.NET Core in-memory test server
- **SQLite** - Lightweight database for Umbraco during tests

## Test Coverage

The integration tests verify:

- ✅ robots.txt endpoint returns successful HTTP responses
- ✅ Multi-site configuration with different hostnames
- ✅ Development environment blocks all bots by default
- ✅ Production environment allows all bots
- ✅ Sitemap URL generation
- ✅ Safe default behavior for unmatched domains
- ✅ Umbraco Cloud Live environment detection (allows all bots)
- ✅ Umbraco Cloud Dev environment detection (blocks all bots)

## Running Tests

Run all integration tests:

```bash
dotnet test tests/Crumpled.RobotsTxt.Tests.Integration
```

Run with detailed output:

```bash
dotnet test tests/Crumpled.RobotsTxt.Tests.Integration --logger "console;verbosity=detailed"
```

## How It Works

### Test Factories

**UmbracoWebApplicationFactory** - Multi-site configuration test factory:
- Uses a temporary SQLite database file (cleaned up after tests)
- Configures Umbraco for unattended installation
- Sets up test-specific robots.txt configuration with multiple sites
- Reuses the same Umbraco instance across all tests in the class (via `IClassFixture`)

**UmbracoCloudLiveWebApplicationFactory** - Cloud Live environment test factory:
- Simulates Umbraco Cloud live environment
- Sets `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=live` environment variable
- Tests default behavior (allows all bots when no sites configured)

**UmbracoCloudDevWebApplicationFactory** - Cloud Dev environment test factory:
- Simulates Umbraco Cloud development environment
- Sets `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=development` environment variable
- Tests safe default behavior (blocks all bots when no sites configured)

### Test Isolation
 per factory: ~15-20 seconds (Umbraco installation)
- Subsequent tests in same class: Fast (reuses same Umbraco instance)
- Cloud tests run sequentially to avoid environment variable conflicts
- Total test suite: ~25-30 secondss to prevent parallel execution:
- `[Collection("CloudLive")]` - Isolates Cloud Live tests
- `[Collection("CloudDev")]` - Isolates Cloud Dev tests

This ensures environment variables don't interfere between test runs.

### Test Configuration

Tests configure different sites and rulesets:

- **localhost:44389** - Development site that blocks all bots
- **localhost:44390** - Production site that allows all bots with sitemap
- **unknown.example.com** - Tests fallback to safe default (block all)

### Performance

- First test run: ~15-20 seconds (Umbraco installation)
- Subsequent tests: Fast (reuses same Umbraco instance)
- Database cleanup: Automatic on dispose

## CI/CD Integration

These tests run automatically:

- ✅ On every push to `develop/v3` branch (ci.yml)
- ✅ Before creating releases (release.yml)
- ✅ Before publishing to NuGet

Failed tests will block deployments, ensuring quality.
