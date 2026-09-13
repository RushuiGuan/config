# Release Notes

## Version 8.0.1

### Changes
* `ApplicationPath.Init()` is now `virtual`, allowing derived types to extend or replace the directory setup — for example to create additional application folders — while still calling `base.Init()`.
* `Init()` creates the roots in `ConfigRoot`, `DataRoot`, `LogRoot` order.

### Dependency Updates
* `Microsoft.Extensions.*` packages updated from `10.0.9` to `10.0.12` to pick up the latest servicing patches.

## Version 8.0.0

### Breaking Changes
* Dropped `netstandard2.0` support. The package now targets `net10.0` only.
* Removed the obsolete `ConfigBase(IConfiguration)` constructor. Derived config classes must call `base(configuration, "<sectionKey>")` instead.
* Removed the obsolete virtual `ConfigBase.Key` property. The section key is now supplied through the constructor rather than by overriding `Key`.

### New Features
* Added `IApplicationPath` interface and `ApplicationPath` implementation to provide standardized OS-aware root paths for data, config, and log directories.
  * `DataRoot`, `ConfigRoot`, and `LogRoot` resolve to OS-appropriate default locations:
    * **Windows (system):** `C:\ProgramData\<subFolders>\data|config|log`
    * **Windows (user):** `%LOCALAPPDATA%\<subFolders>\data|config|log`
    * **macOS (system):** `/Library/Application Support/<subFolders>/data|config|log`
    * **macOS (user):** `~/Library/Application Support/<subFolders>/data|config|log`
    * **Linux (system):** `/var/lib/<subFolders>/data|config|log`
    * **Linux (user):** `~/.config/<subFolders>/data|config|log`
  * `IsSystemPath` indicates whether system-wide paths are in use. System paths typically require elevated permissions to write.
  * System paths are the default. An application opts into user paths with the `{sectionKey}:userMode` setting.
  * Each root can be overridden individually via environment variables or command-line arguments using the configured section key (e.g. `--myapp:dataRoot=/custom/path`).
  * An optional `subSectionKey` groups the three overrides under a nested section (e.g. `myapp:paths:dataRoot`). The nesting applies only to the folder overrides — `userMode` is always read from `{sectionKey}:userMode`.
  * Relative path overrides are resolved against `Environment.CurrentDirectory`, which is suitable for CLI applications where the caller controls the working directory.
  * `Init()` creates the three directories. In user mode on non-Windows platforms each directory is restricted to owner-only access (`rwx------`).
* Added `EnterpriseApplicationPath`, an `IApplicationPath` for the traditional Windows enterprise deployment: `ConfigRoot` is always the binary directory (`AppContext.BaseDirectory`), while `DataRoot` and `LogRoot` come from machine-wide environment variables named by the caller, with the application name appended to keep each application's files separate. When a variable is unset, the root falls back to a `data` / `log` folder beside the binary.

### Changes
* Retired the trimming and NativeAOT annotations. The `UnconditionalSuppressMessage` and `DynamicallyAccessedMembers` attributes have been removed from `ConfigBase`, `Extension.AddConfig`, and `Factory<T>` — they claimed a trim-safety guarantee the library cannot honor, because `ConfigurationBinder.Bind` recurses into nested types those attributes do not preserve. The library makes no trim or AOT safety claim; see `project-mgmt/make-config-aot-safe.tsk.md` for the analysis.
* `Factory<T>` now creates instances with `Activator.CreateInstance` instead of a compiled `Expression` tree, and throws `InvalidOperationException` with a clear message when construction fails.
* The static class `Extension` keeps its name (rather than the conventional `Extensions`) to avoid a silent ABI break for prebuilt callers in a mixed-version dependency graph.
* Added `Microsoft.Extensions.Configuration.CommandLine` and `Microsoft.Extensions.Configuration.EnvironmentVariables` package references, used by the `ApplicationPath` constructors that read path overrides directly from the environment and command line.
