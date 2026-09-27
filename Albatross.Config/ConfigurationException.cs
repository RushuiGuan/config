using System;

namespace Albatross.Config {
	public class ConfigurationException : Exception {
		public ConfigurationException(Type type, string property, string msg) : base($"Invalid config value for class {type.FullName}, property {property}: {msg}") { }
		public ConfigurationException(Type type, string property) : base($"Missing config value for class {type.FullName}, property {property}") { }
		public ConfigurationException(string key) : base($"Missing config value for key: {key}") { }
		public ConfigurationException(string key, string msg) : base($"Invalid config value for key {key}: {msg}") { }
	}
}