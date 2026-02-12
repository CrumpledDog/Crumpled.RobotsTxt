# Crumpled.RobotsTxt

A flexible, configuration-driven robots.txt solution for **Umbraco v13, v14, v15, v16 & v17**

<img src="crumpled-robots-txt.svg" width="150" />

[![Build Status](https://github.com/CrumpledDog/Crumpled.RobotsTxt/actions/workflows/ci.yml/badge.svg?branch=develop/v3)](https://github.com/CrumpledDog/Crumpled.RobotsTxt/actions/workflows/ci.yml) [![NuGet](https://img.shields.io/nuget/v/Crumpled.RobotsTxt.svg)](https://www.nuget.org/packages/Crumpled.RobotsTxt/) [![NuGet Downloads](https://img.shields.io/nuget/dt/Crumpled.RobotsTxt.svg)](https://www.nuget.org/packages/Crumpled.RobotsTxt/)

## Repository Structure

This repository contains:

- **[Crumpled.RobotsTxt](src/Crumpled.RobotsTxt/)** - Main Umbraco package for managing robots.txt configuration ([README](src/Crumpled.RobotsTxt/README.md))
- **[Crumpled.RobotsTxt.Core](src/Crumpled.RobotsTxt.Core/)** - Internal ASP.NET Core robots.txt middleware implementation
- **[Crumpled.RobotsTxt.TestSite](src/Crumpled.RobotsTxt.TestSite/)** - Test Umbraco site for development (Umbraco v14+)
- **[Crumpled.RobotsTxt.TestSite13](src/Crumpled.RobotsTxt.TestSite13/)** - Test Umbraco site for v13 compatibility

## Installation

```console
dotnet add package Crumpled.RobotsTxt
```

📖 **[View Full Documentation & Configuration Guide](src/Crumpled.RobotsTxt/README.md)**

## Key Features

- 🛡️ **Safe by Default** - Blocks all bots by default to prevent accidental indexing
- 🌍 **Multi-Site & Environment-Aware** - Different rules per domain/environment
- 📝 **Flexible Configuration** - Reusable rulesets via appsettings.json
- 🗺️ **Sitemap Integration** - Automatic sitemap URL generation
- ⚙️ **Zero Code Setup** - Auto-registration via Umbraco Composer


## Development

### Test Sites

**[Crumpled.RobotsTxt.TestSite](src/Crumpled.RobotsTxt.TestSite/)** - Umbraco v17 test site with unattended installation. Credentials (not that you really need them) are set in [appsettings.Development.json](src/Crumpled.RobotsTxt.TestSite/appsettings.Development.json)

**[Crumpled.RobotsTxt.TestSite13](src/Crumpled.RobotsTxt.TestSite13/)** - Umbraco v13 test site for backward compatibility testing

### Launch Profiles

The main test site includes three launch profiles:

**1. `Crumpled.RobotsTxt.TestSite` (Development Profile)**
- Tests multi-site robots.txt functionality
- Listens on three ports simultaneously:
  - `https://localhost:44389` - "Stage" site (Development ruleset)
  - `https://localhost:44390` - "Prod" site (Production ruleset)
  - `https://localhost:44391` - Unmatched domain (tests fallback behavior)
- Each URL serves different robots.txt content based on hostname configuration

**2. `Crumpled.RobotsTxt.TestSiteLiveCloud` (Umbraco Cloud Live Simulation)**
- Tests Umbraco Cloud live environment detection
- Sets `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=live`
- Single URL: `https://localhost:44392`
- Environment: `CloudTest`
- Demonstrates Cloud-specific default behavior (allows all bots when no sites configured)

**3. `Crumpled.RobotsTxt.TestSiteDevCloud` (Umbraco Cloud Dev Simulation)**
- Tests Umbraco Cloud development environment detection
- Sets `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=development`
- Single URL: `https://localhost:44393`
- Environment: `CloudTest`
- Demonstrates Cloud dev environment behavior (blocks all bots by default)

### Running the Test Sites

Run with the default profile:
```bash
dotnet run --project src/Crumpled.RobotsTxt.TestSite
```

Or specify a launch profile:
```bash
dotnet run --project src/Crumpled.RobotsTxt.TestSite --launch-profile Crumpled.RobotsTxt.TestSiteLiveCloud
dotnet run --project src/Crumpled.RobotsTxt.TestSite --launch-profile Crumpled.RobotsTxt.TestSiteDevCloud
```

Run the Umbraco v13 test site:
```bash
dotnet run --project src/Crumpled.RobotsTxt.TestSite13
```