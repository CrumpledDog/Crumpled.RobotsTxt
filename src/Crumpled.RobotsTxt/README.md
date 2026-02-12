# Crumpled.RobotsTxt

A flexible, configuration-driven robots.txt solution for Umbraco that protects your non-production environments from search engine indexing by default, while giving you granular control over crawling rules across multiple sites and environments.

<img src="https://raw.githubusercontent.com/CrumpledDog/Crumpled.RobotsTxt/refs/heads/develop/v1/crumpled-robots-txt.svg" width="150" />

## Key Features

- **🛡️ Safe by Default** - Blocks all bots by default to prevent accidental indexing of development, staging, or preview environments
- **🌍 Multi-Site & Environment-Aware** - Configure different robots.txt rules for different domains/hostnames and environments (Production, Development, Staging, etc.)
- **📝 Flexible Rule Configuration** - Define reusable rulesets with Allow/Disallow patterns for different user agents
- **🗺️ Sitemap Integration** - Include sitemap URLs per site
- **☁️ Umbraco Cloud Ready** - Default behaviour designed for Umbraco Cloud - Perfect for hiding those often overlooked *.umbraco.io environment domains.
- **⚙️ Zero Code Setup** - Works out of the box with auto-registration

## Install NuGet package

```console
dotnet add package Crumpled.RobotsTxt
```

## Setup

The package automatically registers itself via a Umbraco Composer. No code changes required!

### Manual Registration (Advanced)

If you prefer to manually register the package in `program.cs`, disable the composer:

```json
"Crumpled": {
  "RobotsTxt": {
    "DisableComposer": true
  }
}
```

Then add to your `program.cs`:

```C#
.AddCrumpledRobotsTxt()
```

## Default Behavior - Protection First

The package prioritizes **protecting your content from unintended indexing**. When no `Sites` are configured, smart defaults kick in:

- **Custom Default**: If you specify a `DefaultRuleset`, that ruleset will be used as the fallback
- **Umbraco Cloud Live Environment**: If the environment variable `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME` equals `"live"`, all bots are allowed by default:
  ```
  User-agent: *
  Allow: /
  ```

- **All Other Environments**: All bots are **blocked by default** for safety - protecting staging, development, and preview environments:
  ```
  User-agent: *
  Disallow: /
  ```

⚠️ **Note:** Once you configure `Sites`, these defaults are ignored and your custom `RuleSets` take full control.

### Unmatched Domains - Additional Protection

When `Sites` are configured, any domain that doesn't match the configured `HostNames` (e.g., temporary preview URLs, forgotten subdomains) will get a protective fallback:

- **Custom Default**: If you specify a `DefaultRuleset`, that ruleset will be used
- **Otherwise**: Blocks all bots for safety:
  ```
  User-agent: *
  Disallow: /
  ```
  
This prevents unintended crawling of staging, preview, or other unlisted domains - ensuring only your explicitly configured production domains are indexed.

## Configuration Example - Multi-Site Setup

Configure different robots.txt rules for different environments and domains using reusable rulesets:

```json
"Crumpled": {
  "RobotsTxt": {
    "DefaultRuleset": "Development",
    "RuleSets": {
      "Production": {
        "Allow": {
          "*" : ["/"],
          "Twitterbot": [ "/" ],
          "facebookexternalhit": [ "/" ]
        },
        "Disallow": {
          "*": [ "/cdn-cgi/challenge-platform/", "/cdn-cgi/email-platform/" ]
        }
      },
      "Development": {
        "Allow": {
          "SemrushBot": [ "/" ],
          "SemrushBot-SA": [ "/" ],
          "SemrushBot-Desktop": [ "/" ],
          "SemrushBot-Mobile": [ "/" ],
          "SiteAuditBot": [ "/" ],
          "PowerMapper": [ "/" ]
        },
        "Disallow": {
          "*": [ "/" ]
        }
      }
    },
    "Sites": {
      "Prod": {
        "HostNames": "www.mysite2.com,mysite.com,localhost:44390",
        "SiteMapDomain": "www.mysite3.com",
        "RuleSet": "Production"
      },
      "Stage": {
        "HostNames": "www.mysite.com,mysite.com,localhost:44389",
        "SiteMapDomain": "www.mysite.com",
        "RuleSet": "Development"
      }
    }
  }
}
```
