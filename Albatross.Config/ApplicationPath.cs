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

		public ApplicationPath(bool useSystemPath, string[] subFolders, IConfiguration configuration, string appPrefix) {
			this.IsSystemPath = useSystemPath;

			var value = configuration.GetSection($"{appPrefix}:dataRoot").Value;
			DataRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "data"]) : GetAbsolutePath(value);
			value = configuration.GetSection($"{appPrefix}:configRoot").Value;
			ConfigRoot = string.IsNullOrEmpty(value) ? GetDefaultPath(useSystemPath, [..subFolders, "config"]) : GetAbsolutePath(value);
			value = configuration.GetSection($"{appPrefix}:logRoot").Value;
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