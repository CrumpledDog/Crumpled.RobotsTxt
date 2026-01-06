# Crumpled.RobotsTxt

This package adds the Crumpled Robots Txt

<img src="crumpled-robots-txt.svg" width="150" />

## Install NuGet package

```console
dotnet add package Crumpled.RobotsTxt --prerelease
```

## Enable in `program.cs`

```C#
.AddCrumpledRobotsTxt("http://mysite.com")
```

## Enable the middleware

```C#
app.UseRobotsTxt();
```

## appsettings.json options (recommended)

```json
    "RobotsTxt": {
      "Allow": {
        "SpecialBot2": [ "/" ]
      },
      "Disallow": {
        "SpecialBot": [ "/" ]
      },
      "Domains": {
        "Prod": {
          "HostNames": "www.mysite2.com,mysite.com,localhost:44390",
          "SiteMapDomain": "www.mysite3.com",
          "IsProduction": false
        },
        "Stage": {
          "HostNames": "www.mysite.com,mysite.com,localhost:44389",
          "SiteMapDomain": "www.mysite.com",
          "IsProduction": true
        }
      }
    }
```

## appsettings.json options (legacy)

```json
"Crumpled": {
  "RobotsTxt": {
    "IsProduction": false,
    "Allow": {
      "SpecialBot2": "/"
    },
    "Disallow": {
      "SpecialBot": "/"
    }
  }
}
```