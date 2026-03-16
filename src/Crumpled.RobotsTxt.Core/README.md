# Crumpled.RobotsTxt.Core

A lightweight, internal implementation of robots.txt middleware for ASP.NET Core, based on the develop branch of [RobotsTxtCore](https://github.com/karl-sjogren/robots-txt-middleware).

## Why This Exists

This is an internal replacement for the `RobotsTxtCore` NuGet package (v3.1.0-preview1), which has been in preview since October 2023 and appears to be stale. Rather than depending on an unmaintained external package, this project provides the same functionality as a lightweight, maintainable internal library.

## Features

- Fluent API for building robots.txt configurations
- Support for multiple user agents, rules, and sitemaps
- Environment-based configuration
- Hostname-based routing support
- ASP.NET Core middleware integration
- Minimal dependencies (only ASP.NET Core)

## Usage

The API is identical to RobotsTxtCore:

```csharp
services.AddStaticRobotsTxt(builder =>
    builder
        .AddSection(section =>
            section
                .AddUserAgent("*")
                .Allow("/")
        )
        .AddSitemap("https://example.com/sitemap.xml")
);

app.UseRobotsTxt();
```

### Content Signals Support

Add Content Signals ([contentsignals.org](https://contentsignals.org/)) to control AI training and content usage:

```csharp
services.AddStaticRobotsTxt(builder =>
    builder
        .AddSection(section =>
            section
                .AddUserAgent("*")
                .WithContentSignal(cs => cs.AllowSearchOnly())  // search=yes, ai-train=no, ai-input=no
                .Allow("/")
        )
        .AddSitemap("https://example.com/sitemap.xml")
);
```

Or specify individual permissions:

```csharp
services.AddStaticRobotsTxt(builder =>
    builder
        .AddSection(section =>
            section
                .AddUserAgent("*")
                .WithContentSignal(cs => cs
                    .AllowAiTrain(false)
                    .AllowSearch(true)
                    .AllowAiInput(false))
                .Allow("/")
        )
);
```

Path-specific content signals:

```csharp
services.AddStaticRobotsTxt(builder =>
    builder
        .AddSection(section =>
            section
                .AddUserAgent("*")
                .WithContentSignal(cs => cs
                    .ForPath("/blog/")
                    .AllowSearch(true)
                    .AllowAiTrain(false))
                .Allow("/blog/")
        )
);
```

Convenience methods:
- `DisallowAll()` - Block all AI actions
- `AllowSearchOnly()` - Allow search indexing only
- `AllowSearchAndAiInput()` - Allow search and AI input (no training)
- `AllowAll()` - Allow all AI actions
