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

To minimize test execution time (~5 seconds vs ~25 seconds), the project includes a pre-initialized SQLite database (`Umbraco.seed.sqlite.db`) that is committed to source control. This seed database contains a fully installed Umbraco instance, eliminating the 15-20 second installation time on every test run.

The seed database is:
- Copied from the TestSite project's working Umbraco database
- Committed to the tests project for fast CI/CD execution
- Automatically copied to each test's temporary database location
- Isolated per test run (each factory uses a unique temp database)

**Test Execution Times:**
- With seed database: ~5-6 seconds for all 10 tests
- Without seed database: ~25-30 seconds (Umbraco must install 3 times)
- Database cleanup: Automatic on dispose

**Updating the Seed Database:**

If the Umbraco schema changes or you need to regenerate the seed database:

```bash
# Copy the latest working database from TestSite
Copy-Item "src\Crumpled.RobotsTxt.TestSite\umbraco\Data\Umbraco.sqlite.db" "tests\Crumpled.RobotsTxt.Tests.Integration\Umbraco.seed.sqlite.db"
```

## CI/CD Integration

These tests run automatically:

- ✅ On every push to `develop/v3` branch (ci.yml)
- ✅ Before creating releases (release.yml)
- ✅ Before publishing to NuGet

Failed tests will block deployments, ensuring quality.
