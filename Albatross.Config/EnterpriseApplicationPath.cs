using System;
using System.IO;

namespace Albatross.Config {
	/// <summary>
	/// An <see cref="IApplicationPath"/> for enterprise deployments that follow the traditional Windows
	/// convention: configuration is shipped alongside the binary, while data and log directories are typically
	/// provided by machine-wide environment variables set on the server.
	/// </summary>
	/// <remarks>
	/// The three roots are resolved as follows:
	/// <list type="bullet">
	///   <item>
	///     <description>
	///     <see cref="ConfigRoot"/> is always <see cref="AppContext.BaseDirectory"/>. Configuration is deployed
	///     with the binary, and environment differences are handled by additional
	///     <c>appsettings.{environment}.json</c> files placed next to the executable rather than by relocating
	///     the config directory.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///     <see cref="DataRoot"/> and <see cref="LogRoot"/> come from the environment variables named by the
	///     constructor arguments. When a variable is set, its value is treated as a shared, machine-wide root and
	///     <c>appName</c> is appended to keep each application's files separate (e.g. a <c>LogDirectory</c> of
	///     <c>D:\Logs</c> yields <c>D:\Logs\{appName}</c>). When the variable is unset (or no name was supplied),
	///     the root falls back to a <c>data</c> / <c>log</c> sub-folder beside the binary; no <c>appName</c>
	///     segment is added there because the binary directory is already unique per deployment.
	///     </description>
	///   </item>
	/// </list>
	/// </remarks>
	public class EnterpriseApplicationPath : IApplicationPath {
		/// <param name="appName">
		/// The application name appended to a machine-wide data/log root to isolate this application's files from
		/// other applications sharing the same environment-variable-provided directory. Ignored when a root falls
		/// back to the binary directory.
		/// </param>
		/// <param name="logDirectoryEnvName">
		/// Name of the environment variable holding the shared log root (e.g. <c>"LogDirectory"</c>). When null,
		/// empty, or unset, the log root falls back to the <c>log</c> folder beside the binary.
		/// </param>
		/// <param name="dataDirectoryEnvName">
		/// Name of the environment variable holding the shared data root (e.g. <c>"DataDirectory"</c>). When null,
		/// empty, or unset, the data root falls back to the <c>data</c> folder beside the binary.
		/// </param>
		public EnterpriseApplicationPath(string appName, string? logDirectoryEnvName, string? dataDirectoryEnvName) {
			this.ConfigRoot = AppContext.BaseDirectory;
			this.DataRoot = GetPath(dataDirectoryEnvName, appName, "data");
			this.LogRoot = GetPath(logDirectoryEnvName, appName, "log");
		}

		/// <summary>Always <c>true</c>; this implementation targets system-wide (machine) deployments.</summary>
		public bool IsSystemPath => true;

		/// <summary>The resolved data directory — a per-application folder under the machine-wide data root, or <c>data</c> beside the binary.</summary>
		public string DataRoot { get; }

		/// <summary>The configuration directory, which is always the binary directory (<see cref="AppContext.BaseDirectory"/>).</summary>
		public string ConfigRoot { get; }

		/// <summary>The resolved log directory — a per-application folder under the machine-wide log root, or <c>log</c> beside the binary.</summary>
		public string LogRoot { get; }


		static string GetPath(string? environmentVariable, string appName, string fallbackFolder) {
			if (!string.IsNullOrEmpty(environmentVariable)) {
				var value = Environment.GetEnvironmentVariable(environmentVariable);
				if (!string.IsNullOrEmpty(value)) {
					return Path.Join(value, appName);
				}
			}
			return Path.Join(AppContext.BaseDirectory, fallbackFolder);
		}


		/// <summary>
		/// Creates the data and log directories if they do not already exist. <see cref="ConfigRoot"/> is not
		/// created because it is the binary directory, which always exists.
		/// </summary>
		/// <exception cref="UnauthorizedAccessException">
		/// Thrown when a directory cannot be created because the process lacks the required permissions.
		/// </exception>
		public void Init() {
			this.EnsureDirectory(this.DataRoot);
			this.EnsureDirectory(this.LogRoot);
		}

		private void EnsureDirectory(string path) {
			try {
				Directory.CreateDirectory(path);
			} catch (UnauthorizedAccessException ex) {
				throw new UnauthorizedAccessException($"'{path}' is not accessible", ex);
			}
		}
	}
}