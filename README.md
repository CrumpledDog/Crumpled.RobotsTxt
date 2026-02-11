# Crumpled.RobotsTxt

A flexible, configuration-driven robots.txt solution for **Umbraco v13, v14, v15, v16 & v17**

<img src="crumpled-robots-txt.svg" width="150" />

## Repository Structure

This repository contains:

- **[Crumpled.RobotsTxt](src/Crumpled.RobotsTxt/)** - Main Umbraco package for managing robots.txt configuration ([README](src/Crumpled.RobotsTxt/README.md))
- **[Crumpled.RobotsTxt.Core](src/Crumpled.RobotsTxt.Core/)** - Internal ASP.NET Core robots.txt middleware implementation
- **[Crumpled.RobotsTxt.TestSite](src/Crumpled.RobotsTxt.TestSite/)** - Test Umbraco site for development

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

**[Crumpled.RobotsTxt.TestSite](src/Crumpled.RobotsTxt.TestSite/)** - Unattended installation, credentials (not that you really need them) are set in [appsettings.Development.json](src/Crumpled.RobotsTxt.TestSite/appsettings.Development.json)

### Launch Profiles

The test site includes two launch profiles:

**1. `Crumpled.RobotsTxt.TestSite` (Development Profile)**
- Tests multi-site robots.txt functionality
- Listens on three ports simultaneously:
  - `https://localhost:44389` - "Stage" site (Development ruleset)
  - `https://localhost:44390` - "Prod" site (Production ruleset)
  - `https://localhost:44391` - Unmatched domain (tests fallback behavior)
- Each URL serves different robots.txt content based on hostname configuration

**2. `Crumpled.RobotsTxt.TestSiteCloud` (Umbraco Cloud Simulation)**
- Tests Umbraco Cloud live environment detection
- Sets `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=live`
- Single URL: `https://localhost:44392`
- Demonstrates Cloud-specific default behavior (allows all bots when no sites configured)

### Running the Test Site

Run with the default profile:
```bash
dotnet run --project src/Crumpled.RobotsTxt.TestSite
```

Or specify a launch profile:
```bash
dotnet run --project src/Crumpled.RobotsTxt.TestSite --launch-profile Crumpled.RobotsTxt.TestSiteCloud
```