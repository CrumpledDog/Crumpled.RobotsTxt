# Crumpled.RobotsTxt

This package adds the Crumpled Robots Txt

<img src="crumpled-robots-txt.svg" width="150" />

## Install NuGet package

```console
dotnet add package Crumpled.RobotsTxt --prerelease
```

## Enable in `program.cs`

```C#
.AddCrumpledRobotsTxt()
```

## Enable the middleware

```C#
app.UseRobotsTxt();
```

## Configuration

```json
"Crumpled": {
  "RobotsTxt": {
    "RuleSets": {
      "Production": {
        "Allow": {
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