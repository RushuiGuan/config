using System;

namespace Albatross.Config {
	/// <summary>
	/// This is created because IHostEnvironment will return production if ASPNETCORE_ENVIRONMENT OR DOTNET_ENVIRONMENT
	/// variable is not set
	/// </summary>
	public class EnvironmentSetting {
		public const string UnknownEnvironment = "Unknown";

		public string Value { get; }
		public string HostName => System.Net.Dns.GetHostName();

		public EnvironmentSetting(string variable) {
			Value = Environment.GetEnvironmentVariable(variable)?.ToLower() ?? UnknownEnvironment;
		}

		public static readonly EnvironmentSetting ASPNETCORE_ENVIRONMENT = new EnvironmentSetting("ASPNETCORE_ENVIRONMENT");
		public static readonly EnvironmentSetting DOTNET_ENVIRONMENT = new EnvironmentSetting("DOTNET_ENVIRONMENT");
		public bool IsProd => string.Equals(Value, "production", StringComparison.InvariantCultureIgnoreCase);
	}
}