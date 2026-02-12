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

Cloud tests use xUnit test collections to prevent parallel execution:
- `[Collection("CloudLive")]` - Isolates Cloud Live tests
- `[Collection("CloudDev")]` - Isolates Cloud Dev tests

This ensures environment variables don't interfere between test runs.

### Test Configuration

Tests configure different sites and rulesets:

- **localhost:44389** - Development site that blocks all bots
- **localhost:44390** - Production site that allows all bots with sitemap
- **unknown.example.com** - Tests fallback to safe default (block all)

### Performance

**Seed Database Optimization:**

To minimize test execution time (~5-7 seconds vs ~25-30 seconds), the project includes pre-initialized SQLite databases that are committed to source control:
- `Umbraco.seed.v13.sqlite.db` - Used by net8.0 tests (Umbraco v13)
- `Umbraco.seed.v17.sqlite.db` - Used by net10.0 tests (Umbraco v17)

These seed databases contain fully installed Umbraco instances, eliminating the 15-20 second installation time on every test run.

The seed databases are:
- Version-specific for Umbraco v13 and v17
- Committed to the tests project for fast CI/CD execution
- Automatically selected based on target framework (conditional compilation)
- Copied to each test's temporary database location
- Isolated per test run (each factory uses a unique temp database)

**Test Execution Times:**
- With seed databases: ~5-7 seconds for all 10 tests (per framework)
- Without seed databases: ~25-30 seconds (Umbraco must install 3 times)
- Total for both frameworks: ~12-18 seconds vs ~50-60 seconds

**Updating the Seed Databases:**

If the Umbraco schema changes or you need to regenerate the seed databases:

```bash
# For Umbraco v13 (from TestSite13)
Copy-Item "src\Crumpled.RobotsTxt.TestSite13\umbraco\Data\Umbraco.sqlite.db" "tests\Crumpled.RobotsTxt.Tests.Integration\Umbraco.seed.v13.sqlite.db"

# For Umbraco v17 (from TestSite)
Copy-Item "src\Crumpled.RobotsTxt.TestSite\umbraco\Data\Umbraco.sqlite.db" "tests\Crumpled.RobotsTxt.Tests.Integration\Umbraco.seed.v17.sqlite.db"
```

## CI/CD Integration

These tests run automatically:

- ✅ On every push to `develop/v3` branch (ci.yml)
- ✅ Before creating releases (release.yml)
- ✅ Before publishing to NuGet

Failed tests will block deployments, ensuring quality.
