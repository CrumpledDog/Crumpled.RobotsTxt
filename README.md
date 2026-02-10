# Crumpled.RobotsTxt

This package adds the Crumpled Robots Txt

<img src="crumpled-robots-txt.svg" width="150" />

## Install NuGet package

```console
dotnet add package Crumpled.RobotsTxt --prerelease
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

## Default Behavior

When no `Sites` are configured, the package uses smart defaults:

- **Umbraco Cloud Live Environment**: If the environment variable `UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME` equals `"live"`, all bots are allowed by default:
  ```
  User-agent: *
  Allow: /
  ```

- **All Other Environments**: All bots are blocked by default for safety:
  ```
  User-agent: *
  Disallow: /
  ```

⚠️ **Note:** Once you configure `Sites`, these defaults are ignored and your custom `RuleSets` take full control.

### Unmatched Domains

When `Sites` are configured, any domain that doesn't match the configured `HostNames` will get a safe fallback:
```
User-agent: *
Disallow: /
```
This prevents unintended crawling of staging, preview, or other unlisted domains.

## Configuration

```json
"Crumpled": {
  "RobotsTxt": {
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