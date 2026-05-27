# Application Path

## Use Case

Applications often need to store files in well-known locations — configuration files, data files, and log files. The right location depends on two factors:

* **Who owns the data** — system-wide data shared across all users, or data belonging to the current user only.
* **The operating system** — each platform has its own convention for where applications should store their files.

`ApplicationPath` resolves this by providing three root paths — `DataRoot`, `ConfigRoot`, and `LogRoot` — computed from OS conventions and optionally overridden by environment variables or command-line arguments.

The `IsSystemPath` property tells the rest of the application whether the paths are system-wide. This matters because writing to system paths typically requires elevated permissions, and the application may need to check or request those permissions before attempting to write.

---

## Default Path Locations

When no override is provided, paths are built from the OS base directory and the `subFolders` segments you supply:

| OS | System path (`IsSystemPath = true`) | User path (`IsSystemPath = false`) |
|----|-----|------|
| Windows | `C:\ProgramData\<subFolders>` | `%LOCALAPPDATA%\<subFolders>` |
| macOS | `/Library/Application Support/<subFolders>` | `~/Library/Application Support/<subFolders>` |
| Linux | `/var/lib/<subFolders>` | `~/.config/<subFolders>` |

The final segment (`data`, `config`, or `log`) is always appended automatically. For example, if `subFolders` is `["mycompany", "myapp"]` on Linux with system paths, the defaults are:

```
DataRoot   → /var/lib/mycompany/myapp/data
ConfigRoot → /var/lib/mycompany/myapp/config
LogRoot    → /var/lib/mycompany/myapp/log
```

---

## Basic Usage

Construct `ApplicationPath` at the entry point of your application, passing the command-line `args` so that overrides can be picked up:

```csharp
var paths = new ApplicationPath(
    useSystemPath: true,
    subFolders: ["mycompany", "myapp"],
    sectionKey: "myapp",
    commandlineArgs: args);

Console.WriteLine(paths.DataRoot);    // e.g. /var/lib/mycompany/myapp/data
Console.WriteLine(paths.ConfigRoot);  // e.g. /var/lib/mycompany/myapp/config
Console.WriteLine(paths.LogRoot);     // e.g. /var/lib/mycompany/myapp/log
Console.WriteLine(paths.IsSystemPath); // true
```

### Registering with Dependency Injection

```csharp
builder.Services.AddSingleton<IApplicationPath>(
    new ApplicationPath(true, ["mycompany", "myapp"], "myapp", args));
```

Inject `IApplicationPath` anywhere in your application:

```csharp
public class DataService {
    private readonly IApplicationPath paths;
    public DataService(IApplicationPath paths) {
        this.paths = paths;
    }
    public string GetDataFilePath(string fileName) =>
        Path.Combine(paths.DataRoot, fileName);
}
```

### Using an existing IConfiguration

If your application already has an `IConfiguration` instance (e.g. inside a hosted service or DI setup), use the overload that accepts it directly:

```csharp
var paths = new ApplicationPath(configuration, true, ["mycompany", "myapp"], "myapp");
```

---

## Initializing the Directories

Constructing an `ApplicationPath` only resolves the path strings — it does not create the directories. Call `Init()` once at startup to ensure all three directories exist before the application tries to use them:

```csharp
var paths = new ApplicationPath(true, ["mycompany", "myapp"], "myapp", args);
paths.Init();
```

`Init()` calls `Directory.CreateDirectory` for each root, which is a no-op if the directory already exists, so it is safe to call on every startup.

### Permissions

On non-Windows systems, user-path directories (`IsSystemPath = false`) are created with mode `0700` (owner read/write/execute only), keeping the application's private data inaccessible to other users.

System-path directories (`IsSystemPath = true`) require elevated permissions. If the process does not have sufficient rights, `Init()` throws an `UnauthorizedAccessException` with the path that failed. On Linux/macOS this typically means the service must run as root or have the appropriate grants. On Windows the process must run as Administrator.

---

## Overriding Paths

Any or all of the three roots can be overridden at runtime via environment variables or command-line arguments using the `sectionKey` you specified.

### Command-line override

```
myapp.exe --myapp:dataRoot=/opt/myapp/data
```

### Environment variable override

Environment variables use double underscore (`__`) as the section separator:

```
myapp__dataRoot=/opt/myapp/data
```

Partial overrides are supported — only the specified roots are replaced; unspecified roots continue to use the OS defaults.

### Relative paths (CLI applications)

Relative paths are resolved against `Environment.CurrentDirectory`, which is the working directory the caller had when they launched the application. This makes relative paths practical for CLI tools:

```
anchor-cli --myapp:configRoot=.\config
```

> **Note:** Avoid relative path overrides in long-running services (Windows Service, systemd) where the working directory is set by the service manager and may be unpredictable.

---

## Static Helpers

`ApplicationPath` exposes its OS detection logic as public static methods if you need the base paths directly:

```csharp
// Returns the system-wide base path for the current OS
string systemRoot = ApplicationPath.GetSystemRootPath();

// Returns the current user's base path for the current OS
string userRoot = ApplicationPath.GetUserRootPath();

// Builds a full default path from the base dir and sub-folder segments
string path = ApplicationPath.GetDefaultPath(useSystemPath: true, ["mycompany", "myapp", "data"]);

// Normalizes a path: returns it unchanged if already absolute,
// or resolves it against the current directory if relative
string absolute = ApplicationPath.GetAbsolutePath("relative/path");
```
