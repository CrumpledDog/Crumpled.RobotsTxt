# Crumpled.RobotsTxt

This package adds the Crumpled Robots Txt

## Install NuGet package

```console
dotnet add package Crumpled.RobotsTxt --prerelease
```

## Enable in `program.cs`

```C#
.AddCrumpledRobotsTxt("http://mysite.com")
```
## Enabled the middleware

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