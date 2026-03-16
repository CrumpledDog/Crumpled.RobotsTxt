# Crumpled.RobotsTxt

A flexible, configuration-driven robots.txt solution for **Umbraco v13, v14, v15, v16 & v17** that protects your non-production environments from search engine indexing by default, while giving you granular control over crawling rules across multiple sites and environments.

## Key Features

- **🛡️ Safe by Default** - Blocks all bots by default to prevent accidental indexing of development, staging, or preview environments
- **🌍 Multi-Site & Environment-Aware** - Configure different robots.txt rules for different domains/hostnames and environments (Production, Development, Staging, etc.)
- **📝 Flexible Rule Configuration** - Define reusable rulesets with Allow/Disallow patterns for different user agents
- **🤖 Content Signals Support** - Control AI training and content usage with [Content Signals](https://contentsignals.org/) directives
- **🔄 Hot Reload** - Configuration changes are automatically picked up without requiring an application restart
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
    "DefaultRuleset": "NonProduction",
    "RuleSets": { // There can be multiple rulesets for complex scenarios!
      "Production": {
        "Allow": {
          "*" : ["/"]
        },
        "Disallow": {
          "*": [ "/cdn-cgi/challenge-platform/", "/cdn-cgi/email-platform/" ]
        }
      },
      "NonProduction": { 
        "Allow": {
          "SemrushBot": [ "/" ],
          "SemrushBot-SA": [ "/" ],
          "SemrushBot-Desktop": [ "/" ],
          "SemrushBot-Mobile": [ "/" ],
          "SiteAuditBot": [ "/" ]
        },
        "Disallow": {
          "*": [ "/" ]
        }
      }
    },
    "Sites": {
      "Prod": {
        "HostNames": "www.mysite.com",
        "SiteMapDomain": "www.mysite.com",
        "RuleSet": "Production"
      },
      "AnotherProd": {
        "HostNames": "www.anothermysite.com",
        "SiteMapDomain": "www.anothermysite.com",
        "RuleSet": "Production" // or can define alternate production ruleset for this site
      },
      "Stage": {
        "HostNames": "mysite-staging-uksouth01.umbraco.io,staging.mysite.com", 
        "SiteMapDomain": "staging.mysite.com",
        "RuleSet": "NonProduction"
      },
      "Dev": {
        "HostNames": "mysite-dev-uksouth01.umbraco.io,dev.mysite.com",
        "SiteMapDomain": "dev.mysite.com",
        "RuleSet": "NonProduction"
      }
    }
  }
}
```

## Content Signals Support

Content Signals ([contentsignals.org](https://contentsignals.org/)) is Cloudflare's implementation for controlling how automated systems (AI crawlers, search engines) use your content. **Content-Signal directives are restrictions on Allow rules only** and declare permissions for:

- **ai-train**: Training or fine-tuning AI models
- **search**: Building search indexes and providing search results
- **ai-input**: Inputting content into AI models (RAG, grounding, generative AI search)

**Important:** A Content-Signal directive is declared once per User-agent section and applies to **all Allow directives** for that user-agent. Multiple paths in an Allow rule will all inherit the same Content-Signal.

### How Content Signals Work

Content Signals are declare permissions at the **User-agent level**. When you have multiple Allow paths for a user-agent, they all share the same Content-Signal:

```json
"Allow": {
  "googlebot": {
    "Paths": ["/blog", "/news"],
    "ContentSignal": {
      "AiTrain": false,
      "Search": true,
      "AiInput": false
    }
  }
}
```

This generates:
```
User-agent: googlebot
Content-Signal: ai-train=no, search=yes, ai-input=no
Allow: /blog
Allow: /news
```

Both `/blog` and `/news` paths are covered by the single Content-Signal directive.

### Content Signal Instructions Header

You can optionally include a legal header at the top of your robots.txt that explains the Content Signal terms and conditions. Enable this per ruleset:

```json
"RuleSets": {
  "Production": {
    "IncludeContentSignalInstructions": true,
    "ContentSignal": {
      "AiTrain": false,
      "Search": true,
      "AiInput": false
    },
    "Allow": {
      "*": ["/"]
    }
  }
}
```

This adds a comprehensive header explaining the Content Signal license terms, including references to EU Directive 2019/790 on copyright. The instructions clarify:
- What constitutes agreement (yes) and restriction (no)
- Definitions of search, ai-input, and ai-train
- Legal basis under EU copyright law

**Note:** Only enable this if you're using Content Signals in that ruleset, as it adds ~35 lines to the top of your robots.txt.

### Configuration within RuleSets

Content Signals are configured within RuleSets and **only apply to Allow directives**. Disallow rules never include Content-Signal directives.

#### Default ContentSignal for All Allow Rules

Configure a default ContentSignal at the RuleSet level that applies to all Allow rules:

```json
"RuleSets": {
  "Production": {
    "ContentSignal": {
      "AiTrain": true,
      "Search": true,
      "AiInput": true
    },
    "Allow": {
      "*": ["/"],
      "Googlebot": ["/"]
    },
    "Disallow": {
      "*": ["/admin/"]
    }
  }
}
```

Both `*` and `Googlebot` Allow rules will get the same Content-Signal.

#### Agent-Specific ContentSignal

Override the default ContentSignal for specific user agents:

```json
"RuleSets": {
  "Production": {
    "IncludeContentSignalInstructions": true,
    "ContentSignal": {
      "AiTrain": true,
      "Search": true,
      "AiInput": true
    },
    "Allow": {
      "OAI-SearchBot": {
        "Paths": ["/"],
        "ContentSignal": {
          "AiTrain": false,
          "Search": true,
          "AiInput": false
        }
      },
      "googlebot": {
        "Paths": ["/blog", "/news"],
        "ContentSignal": {
          "AiTrain": false,
          "Search": true,
          "AiInput": false
        }
      },
      "*": ["/"]
    },
    "Disallow": {
      "*": ["/cdn-cgi/"]
    }
  }
}
```

This generates:
```
# As a condition of accessing this website, you agree to abide by
# the following content signals:
# ... (legal header text) ...

User-agent: googlebot
Content-Signal: ai-train=no, search=yes, ai-input=no
Allow: /blog
Allow: /news

User-agent: OAI-SearchBot
Content-Signal: ai-train=no, search=yes, ai-input=no
Allow: /

User-agent: *
Content-Signal: ai-train=yes, search=yes, ai-input=yes
Allow: /

User-agent: *
Disallow: /cdn-cgi/
```

**Notice:** 
- Specific user agents (googlebot, OAI-SearchBot) appear before the wildcard `*`
- Each user-agent gets its own ContentSignal - googlebot and OAI-SearchBot have restricted permissions, while `*` allows everything
- The legal header is included because `IncludeContentSignalInstructions: true`
- googlebot's single Content-Signal applies to both `/blog` and `/news` paths

#### Simple and Complex Allow Rules

You can mix simple array format and complex object format in the same RuleSet:

- **Simple format**: `"UserAgent": ["/path1", "/path2"]` - Uses default ContentSignal from RuleSet
- **Complex format**: `"UserAgent": { "Paths": [...], "ContentSignal": {...} }` - Uses agent-specific ContentSignal

```json
"RuleSets": {
  "Production": {
    "ContentSignal": {
      "AiTrain": true,
      "Search": true,
      "AiInput": true
    },
    "Allow": {
      "Googlebot": ["/"],  // Simple - uses default ContentSignal (ai-train=yes)
      "OAI-SearchBot": {  // Complex - overrides with ai-train=no
        "Paths": ["/"],
        "ContentSignal": {
          "AiTrain": false,
          "Search": true,
          "AiInput": false
        }
      }
    }
  }
}
```

This lets you set a permissive default for most bots while restricting specific ones like AI search crawlers.

### User-Agent Ordering

Specific user agents are always ordered alphabetically and appear before the wildcard `*`. This follows robots.txt best practices where more specific rules should be evaluated before general rules.

```json
"Allow": {
  "OAI-SearchBot": ["/"],
  "googlebot": ["/blog"],
  "*": ["/"]
}
```

Will always output in this order:
```
User-agent: googlebot
...

User-agent: OAI-SearchBot
...

User-agent: *
...
```

### Common Policies

**Allow Search Only** (no AI):
```json
"ContentSignal": {
  "AiTrain": false,
  "Search": true,
  "AiInput": false
}
```

**Allow Search & AI Input** (no training):
```json
"ContentSignal": {
  "AiTrain": false,
  "Search": true,
  "AiInput": true
}
```

**Allow All**:
```json
"ContentSignal": {
  "AiTrain": true,
  "Search": true,
  "AiInput": true
}
```

**Disallow All** (most restrictive):
```json
"ContentSignal": {
  "AiTrain": false,
  "Search": false,
  "AiInput": false
}
```
