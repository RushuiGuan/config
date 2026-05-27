# Release Notes

## Version 8.0.0

### Breaking Changes
* Dropped `netstandard2.0` support. Minimum target framework is now `net8.0`.

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
  * Each root can be overridden individually via environment variables or command-line arguments using the configured section key (e.g. `--myapp:dataRoot=/custom/path`).
  * Relative path overrides are resolved against `Environment.CurrentDirectory`, which is suitable for CLI applications where the caller controls the working directory.
