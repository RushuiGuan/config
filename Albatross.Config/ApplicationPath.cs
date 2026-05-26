using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Albatross.Config {
	public interface IApplicationPath {
		bool IsSystemPath { get; }
		string DataRoot { get; }
		string ConfigRoot { get; }
		string LogRoot { get; }
	}
	public class ApplicationPath : IApplicationPath {
		public static string GetSystemRootPath() {
			if (OperatingSystem.IsWindows()) {
				// windows: c:\ProgramData
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			} else if (OperatingSystem.IsMacOS()) {
				// mac: /Library/Application Support
				return Path.Join("/Library", "Application Support");
			} else {
				// linux: /var/lib
				return Path.Join("/var", "lib");
			}
		}
		public static string GetUserRootPath() {
			if (OperatingSystem.IsWindows()) {
				// windows: ~\AppData\Local
				return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			} else {
				// linux: ~/.config
				// mac: ~/Library/Application Support
				return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}
		}
		public static string GetDefaultPath(bool useSystemPath, string[] subFolders) {
			if (useSystemPath) {
				return Path.Join([GetSystemRootPath(), .. subFolders]);
			} else {
				return Path.Join([GetUserRootPath(), .. subFolders]);
			}
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
		/// <param name="commandlineArgs">The command-line arguments passed to the application (i.e. <c>args</c> from <c>Main</c>).</param>
		public ApplicationPath(bool useSystemPath, string[] subFolders, string sectionKey, string[] commandlineArgs) {
			this.IsSystemPath = useSystemPath;
			var builder = new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(commandlineArgs);
			var configuration = builder.Build();
			var value = configuration.GetSection($"{sectionKey}:dataRoot").Value;
			DataRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "data"]) : GetAbsolutePath(value);
			value = configuration.GetSection($"{sectionKey}:configRoot").Value;
			ConfigRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "config"]) : GetAbsolutePath(value);
			value = configuration.GetSection($"{sectionKey}:logRoot").Value;
			LogRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "log"]) : GetAbsolutePath(value);
		}


		public bool IsSystemPath {
			get;
		}
		public string DataRoot {
			get;
		}
		public string ConfigRoot {
			get;
		}
		public string LogRoot {
			get;
		}
	}
}