using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Albatross.Config {
	/// <summary>
	/// An <see cref="IApplicationPath"/> that roots each of the data, config, and log directories under the
	/// OS-standard location for application state, following the cross-platform convention used by installed
	/// commercial software.
	/// </summary>
	/// <remarks>
	/// This is the general-purpose implementation, intended for applications that keep their data, configuration,
	/// and logs in separate, well-known per-application directories rather than beside the binary. Each root is
	/// built as <c>{OS base}/{subFolders}/{data|config|log}</c>, where the OS base is chosen by
	/// <see cref="IsSystemPath"/>:
	/// <list type="bullet">
	///   <item><description>System mode — machine-wide state (<c>C:\ProgramData</c>, <c>/var/lib</c>, <c>/Library/Application Support</c>); the default, and typically requires elevated permissions to write.</description></item>
	///   <item><description>User mode — per-user state (<c>%LOCALAPPDATA%</c>, <c>~/.config</c>); opted into via <c>{sectionKey}:userMode</c>.</description></item>
	/// </list>
	/// Any individual root can be overridden per application through the <c>configRoot</c>, <c>dataRoot</c>, and
	/// <c>logRoot</c> configuration keys (see the constructors). For the enterprise pattern where configuration
	/// ships next to the binary and data/log come from shared machine-wide directories, use
	/// <c>EnterpriseApplicationPath</c> instead.
	/// </remarks>
	public class ApplicationPath : IApplicationPath {
		/// <summary>
		/// Returns the OS-standard root for machine-wide (system) application state:
		/// <c>C:\ProgramData</c> on Windows, <c>/Library/Application Support</c> on macOS, and <c>/var/lib</c> on Linux.
		/// </summary>
		public static string GetSystemRootPath() {
			if (System.OperatingSystem.IsWindows()) {
				// windows: c:\ProgramData
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			} else if (System.OperatingSystem.IsMacOS()) {
				// mac: /Library/Application Support
				return Path.Join("/Library", "Application Support");
			} else {
				// linux: /var/lib
				return Path.Join("/var", "lib");
			}
		}

		/// <summary>
		/// Returns the OS-standard root for per-user application state:
		/// <c>%LOCALAPPDATA%</c> on Windows and the user's application-data folder (e.g. <c>~/.config</c>) on macOS/Linux.
		/// </summary>
		public static string GetUserRootPath() {
			if (System.OperatingSystem.IsWindows()) {
				// windows: ~\AppData\Local
				return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			} else {
				// linux: ~/.config
				// mac: ~/Library/Application Support
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}
		}

		/// <summary>
		/// Joins <paramref name="subFolders"/> onto the system or user OS root (per <paramref name="useSystemPath"/>)
		/// to form a default root path.
		/// </summary>
		/// <param name="useSystemPath">When <c>true</c>, builds on <see cref="GetSystemRootPath"/>; otherwise on <see cref="GetUserRootPath"/>.</param>
		public static string GetDefaultPath(bool useSystemPath, string[] subFolders) {
			if (useSystemPath) {
				return Path.Join([GetSystemRootPath(), .. subFolders]);
			} else {
				return Path.Join([GetUserRootPath(), .. subFolders]);
			}
		}

		/// <summary>
		/// Determines whether paths should be rooted under the system-wide data directory by reading the
		/// <c>{sectionKey}:userMode</c> setting. System path is the default: when <c>userMode</c> is absent
		/// or <c>false</c>, this returns <c>true</c>; set <c>userMode</c> to <c>true</c> to opt into user paths.
		/// </summary>
		public static bool UseSystemPath(IConfiguration configuration, string sectionKey)
			=> !configuration.GetValue<bool>($"{sectionKey}:userMode");

		/// <summary>
		/// Reads a configuration value for one of the path overrides (<c>configRoot</c>, <c>dataRoot</c>, <c>logRoot</c>).
		/// When <paramref name="subSectionKey"/> is supplied, the value is read from the nested section
		/// (<c>{sectionKey}:{subSectionKey}:{name}</c>); otherwise it is read directly from <c>{sectionKey}:{name}</c>.
		/// </summary>
		/// <param name="subSectionKey">
		/// An optional nested section under <paramref name="sectionKey"/> that groups the path overrides. This nesting applies
		/// only to the folder overrides — <c>userMode</c> is always read from <c>{sectionKey}:userMode</c>.
		/// </param>
		public static string? GetConfigValue(IConfiguration configuration, string sectionKey, string? subSectionKey, string name) {
			string path;
			if (string.IsNullOrEmpty(subSectionKey)) {
				path = $"{sectionKey}:{name}";
			} else {
				path = $"{sectionKey}:{subSectionKey}:{name}";
			}
			return configuration.GetSection(path).Value;
		}

		/// <summary>
		/// Converts a path to an absolute path. Relative paths are resolved against <see cref="Environment.CurrentDirectory"/>.
		/// This is appropriate for CLI applications where the working directory is set by the caller in the terminal.
		/// Avoid using relative paths in services (Windows Service, systemd) where the working directory is unpredictable.
		/// </summary>
		public static string GetAbsolutePath(string path) {
			if (!Path.IsPathRooted(path)) {
				return Path.GetFullPath(path);
			} else {
				return path;
			}
		}

		/// <summary>
		/// Creates an <see cref="ApplicationPath"/> by reading path overrides from environment variables and command-line arguments.
		/// If a root path is not specified, it is derived from the OS-appropriate base directory combined with <paramref name="subFolders"/>.
		/// </summary>
		/// <param name="useSystemPath">
		/// When <c>true</c>, paths are rooted under the system-wide data directory (e.g. <c>C:\ProgramData</c>, <c>/var/lib</c>).
		/// When <c>false</c>, paths are rooted under the current user's data directory (e.g. <c>%LOCALAPPDATA%</c>, <c>~/.config</c>).
		/// Note that system paths typically require elevated permissions to write.
		/// </param>
		/// <param name="subFolders">
		/// Sub-folder segments appended to the OS base directory to form the default root (e.g. <c>["mycompany", "myapp"]</c>).
		/// Each of <c>data</c>, <c>config</c>, and <c>log</c> is then appended as the final segment.
		/// </param>
		/// <param name="sectionKey">
		/// The configuration section key used to look up path overrides (e.g. <c>"myapp"</c> maps to <c>myapp:dataRoot</c>,
		/// <c>myapp:configRoot</c>, and <c>myapp:logRoot</c>). Overrides can be passed as environment variables or command-line arguments.
		/// </param>
		/// <param name="subSectionKey">
		/// An optional nested section under <paramref name="sectionKey"/> that groups the path overrides (e.g. <c>"myapp"</c> with
		/// <c>"paths"</c> maps to <c>myapp:paths:dataRoot</c>). When <c>null</c> or empty, overrides are read directly under
		/// <paramref name="sectionKey"/>. This nesting applies only to the folder overrides; it does not affect <c>userMode</c>.
		/// </param>
		/// <param name="commandlineArgs">The command-line arguments passed to the application (i.e. <c>args</c> from <c>Main</c>).</param>
		public ApplicationPath(bool useSystemPath, string[] subFolders, string sectionKey, string? subSectionKey, string[] commandlineArgs)
			: this(new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(commandlineArgs).Build(), useSystemPath, subFolders, sectionKey, subSectionKey) {
		}

		/// <summary>
		/// The core constructor all other overloads chain to. For each of <c>configRoot</c>, <c>dataRoot</c>, and
		/// <c>logRoot</c>, an override read from <paramref name="configuration"/> takes precedence (resolved to an
		/// absolute path); otherwise the root defaults to <c>{OS base}/{subFolders}/{config|data|log}</c>.
		/// </summary>
		/// <param name="useSystemPath">
		/// When <c>true</c>, roots are built under the machine-wide OS base (e.g. <c>C:\ProgramData</c>, <c>/var/lib</c>);
		/// when <c>false</c>, under the per-user OS base (e.g. <c>%LOCALAPPDATA%</c>, <c>~/.config</c>).
		/// </param>
		/// <param name="subFolders">
		/// Sub-folder segments appended to the OS base directory to form the default root (e.g. <c>["mycompany", "myapp"]</c>).
		/// Each of <c>data</c>, <c>config</c>, and <c>log</c> is then appended as the final segment.
		/// </param>
		/// <param name="sectionKey">
		/// The configuration section key used to look up the path overrides (e.g. <c>"myapp"</c> maps to
		/// <c>myapp:dataRoot</c>, <c>myapp:configRoot</c>, <c>myapp:logRoot</c>).
		/// </param>
		/// <param name="subSectionKey">
		/// An optional nested section under <paramref name="sectionKey"/> that groups the path overrides (e.g. <c>"myapp"</c>
		/// with <c>"paths"</c> maps to <c>myapp:paths:dataRoot</c>). When <c>null</c> or empty, overrides are read directly
		/// under <paramref name="sectionKey"/>.
		/// </param>
		public ApplicationPath(IConfiguration configuration, bool useSystemPath, string[] subFolders, string sectionKey, string? subSectionKey) {
			this.IsSystemPath = useSystemPath;
			var value = GetConfigValue(configuration, sectionKey, subSectionKey, "configRoot");
			ConfigRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "config"]) : GetAbsolutePath(value);

			value = GetConfigValue(configuration, sectionKey, subSectionKey, "dataRoot");
			DataRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "data"]) : GetAbsolutePath(value);

			value = GetConfigValue(configuration, sectionKey, subSectionKey, "logRoot");
			LogRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "log"]) : GetAbsolutePath(value);
		}

		/// <summary>
		/// Creates an <see cref="ApplicationPath"/> from an existing <paramref name="configuration"/>, deriving the
		/// system-vs-user path mode from the <c>{sectionKey}:userMode</c> setting via <see cref="UseSystemPath"/>.
		/// </summary>
		/// <param name="subFolders">
		/// Sub-folder segments appended to the OS base directory to form the default root (e.g. <c>["mycompany", "myapp"]</c>).
		/// Each of <c>data</c>, <c>config</c>, and <c>log</c> is then appended as the final segment.
		/// </param>
		/// <param name="sectionKey">
		/// The configuration section key used to look up <c>userMode</c> and the path overrides
		/// (<c>dataRoot</c>, <c>configRoot</c>, <c>logRoot</c>).
		/// </param>
		/// <param name="subSectionKey">
		/// An optional nested section under <paramref name="sectionKey"/> that groups the path overrides (e.g. <c>"myapp"</c> with
		/// <c>"paths"</c> maps to <c>myapp:paths:dataRoot</c>). When <c>null</c> or empty, overrides are read directly under
		/// <paramref name="sectionKey"/>. This nesting applies only to the folder overrides; <c>userMode</c> is always read from <c>{sectionKey}:userMode</c>.
		/// </param>
		public ApplicationPath(IConfiguration configuration, string[] subFolders, string sectionKey, string? subSectionKey)
			: this(configuration, UseSystemPath(configuration, sectionKey), subFolders, sectionKey, subSectionKey) {
		}

		/// <summary>
		/// Creates an <see cref="ApplicationPath"/> by building configuration from environment variables and
		/// command-line arguments, then deriving the system-vs-user path mode from the <c>{sectionKey}:userMode</c> setting.
		/// </summary>
		/// <param name="subFolders">
		/// Sub-folder segments appended to the OS base directory to form the default root (e.g. <c>["mycompany", "myapp"]</c>).
		/// Each of <c>data</c>, <c>config</c>, and <c>log</c> is then appended as the final segment.
		/// </param>
		/// <param name="sectionKey">
		/// The configuration section key used to look up <c>userMode</c> and the path overrides
		/// (<c>dataRoot</c>, <c>configRoot</c>, <c>logRoot</c>).
		/// </param>
		/// <param name="subSectionKey">
		/// An optional nested section under <paramref name="sectionKey"/> that groups the path overrides (e.g. <c>"myapp"</c> with
		/// <c>"paths"</c> maps to <c>myapp:paths:dataRoot</c>). When <c>null</c> or empty, overrides are read directly under
		/// <paramref name="sectionKey"/>. This nesting applies only to the folder overrides; <c>userMode</c> is always read from <c>{sectionKey}:userMode</c>.
		/// </param>
		/// <param name="commandlineArgs">The command-line arguments passed to the application (i.e. <c>args</c> from <c>Main</c>).</param>
		public ApplicationPath(string[] subFolders, string sectionKey, string? subSectionKey, string[] commandlineArgs)
			: this(new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(commandlineArgs).Build(), subFolders, sectionKey, subSectionKey) {
		}

		/// <summary><c>true</c> when roots are built under the machine-wide OS base; <c>false</c> when under the per-user base.</summary>
		public bool IsSystemPath { get; }

		/// <summary>The resolved data directory: the <c>dataRoot</c> override, or <c>{OS base}/{subFolders}/data</c>.</summary>
		public string DataRoot { get; }

		/// <summary>The resolved configuration directory: the <c>configRoot</c> override, or <c>{OS base}/{subFolders}/config</c>.</summary>
		public string ConfigRoot { get; }

		/// <summary>The resolved log directory: the <c>logRoot</c> override, or <c>{OS base}/{subFolders}/log</c>.</summary>
		public string LogRoot { get; }

		private void EnsureDirectory(string path) {
			try {
				Directory.CreateDirectory(path);
				if (!IsSystemPath && !OperatingSystem.IsWindows()) {
					File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
				}
			} catch (UnauthorizedAccessException ex) when (IsSystemPath) {
				throw new UnauthorizedAccessException($"'{path}' is not accessible", ex);
			}
		}

		/// <summary>
		/// Creates the data, config, and log directories if they do not already exist. In user mode on non-Windows
		/// platforms each directory is restricted to owner-only access (<c>rwx------</c>).
		/// </summary>
		/// <exception cref="UnauthorizedAccessException">
		/// Thrown in system mode when a directory cannot be created because the process lacks the required
		/// permissions. In user mode this condition is ignored.
		/// </exception>
		public virtual void Init() {
			EnsureDirectory(this.ConfigRoot);
			EnsureDirectory(this.DataRoot);
			EnsureDirectory(this.LogRoot);
		}
	}
}