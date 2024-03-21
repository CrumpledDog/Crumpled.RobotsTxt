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

## Multiple domain support for the sitemap url can be set 

```json
"SiteMapDomains": {
    "www.mysite.com": [ "www.mysite.com", "mysite.com", "localhost:44389" ],
    "www.mysite2.com": [ "www.mysite2.com", "mysite2.com", "localhost:44390" ]
    }
```

## Enable the middleware

```C#
app.UseRobotsTxt();
```

## appsettings.json options

```json
"RobotsTxt": {
    "IsProduction": false,
    "Allow": {
        "SpecialBot2": "/"
    },
    "Disallow": {
        "SpecialBot": "/"
    }
}
```