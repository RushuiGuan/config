# Albatross.Config

`Albatross.Config` is a .NET library that simplifies strongly-typed configuration and application path management for services and CLI tools.

## What it provides

**Strongly-typed configuration** — Define configuration classes that read directly from `IConfiguration` and inject as dependencies, without the boilerplate of `IOptions<>`. Config classes can reach across section boundaries to share values like connection strings or endpoints between libraries.

**Application path management** — Resolve OS-appropriate directories for data, config, and log files using `ApplicationPath`. Paths follow platform conventions (Windows, macOS, Linux) and can be overridden via environment variables or command-line arguments without code changes.

**Safe hosting environment defaults** — A custom `IHostEnvironment` implementation that returns `"Unknown"` when no environment variable is set, avoiding the silent default-to-`production` behavior in the standard .NET host.

## Get started

Install the NuGet package:

```
dotnet add package Albatross.Config
```

Register a config class:

```csharp
builder.Services.AddConfig<MyAppConfig>();
```

Set up application paths:

```csharp
var paths = new ApplicationPath(useSystemPath: false, ["mycompany", "myapp"], "myapp", args);
paths.Init();
builder.Services.AddSingleton<IApplicationPath>(paths);
```

## Articles

- [.NET Hosting Environment](articles/hosting-env.md)
- [Configure the Application](articles/config.md)
- [Albatross.Config vs IOptions](articles/the-comparison.md)
- [Application Path](articles/application-path.md)
