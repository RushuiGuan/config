using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Albatross.Config {
	// the class name Extension cannot be renamed to Extensions (standard) because of backward compability reason.
	public static class Extension {
		/// <summary>
		/// Registration for the Configuration Class C.  C has to have the base class of <see cref="ConfigBase"/> and it also requires a constructor with
		/// a single parameter of type <see cref="IConfiguration"/>
		/// The call will run the validate method right after the object creation.  It is critical to validate configuration data.
		/// A misspelled property name in the appsettings.json file could lead to null config values.
		/// </summary>
		/// <typeparam name="T">The configuration class</typeparam>
		/// <param name="services">The ServiceCollection instance</param>
		/// <param name="singleton"></param>
		/// <returns>The service collection instance</returns>
		public static IServiceCollection AddConfig<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]T>(this IServiceCollection services, bool singleton = true) where T : ConfigBase {
			if (singleton) {
				services.TryAddSingleton<T>(provider => Factory<T>.CreateAndValidate(provider.GetRequiredService<IConfiguration>()));
			} else {
				services.TryAddScoped<T>(provider => Factory<T>.CreateAndValidate(provider.GetRequiredService<IConfiguration>()));
			}
			return services;
		}

		/// <summary>
		/// Note that if TInterface is used, there is no longer the requirement that T must have a constructor with IConfiguration parameter.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="singleton"></param>
		/// <typeparam name="TInterface"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IServiceCollection AddConfig<TInterface, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors| DynamicallyAccessedMemberTypes.PublicProperties)] T>(this IServiceCollection services, bool singleton = true)
			where T : ConfigBase, TInterface
			where TInterface : class {
			if (singleton) {
				services.TryAddSingleton<T>();
				services.TryAddSingleton<TInterface>(provider => {
					var config = provider.GetRequiredService<T>();
					config.Validate();
					return config;
				});
			} else {
				services.TryAddScoped<T>();
				services.TryAddScoped<TInterface>(provider => {
					var config = provider.GetRequiredService<T>();
					config.Validate();
					return config;
				});
			}
			return services;
		}

		const string Slash = "/";

		/// <summary>
		/// For C# the HttpClient class will remove any relative path if the BaseUrl does not end with a slash.  For example: http://localhost/beezy will become 
		/// http://localhost unless base url is set as http://localhost/beezy/
		/// For the request url, if it starts with a slash, it will be considered as a root url.  By default, we shouldn't use any slash in the request url.
		/// The call will append Slack '/' to the endpoint by default if it doesn't already end with it.  If this behavior is not desired, set ensureTrailingSlash 
		/// to false
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="name"></param>
		/// <param name="ensureTrailingSlash"></param>
		/// <returns></returns>
		public static string? GetEndPoint(this IConfiguration configuration, string name, bool ensureTrailingSlash = true) {
			string? value = configuration.GetSection($"endpoints:{name}")?.Value;
			if (value != null && !value.EndsWith(Slash) && ensureTrailingSlash) {
				value = value + Slash;
			}
			return value;
		}


		public static string GetRequiredEndPoint(this IConfiguration configuration, string name, bool ensureTrailingSlash = true) {
			string section = $"endpoints:{name}";
			string? value = configuration.GetSection(section)?.Value;
			if (value != null && !value.EndsWith(Slash) && ensureTrailingSlash) {
				value = value + Slash;
			}
			return value ?? throw new ConfigurationException(section);
		}

		public static string GetRequiredConnectionString(this IConfiguration configuration, string name) {
			string? value = configuration.GetConnectionString(name);
			return value ?? throw new ConfigurationException($"connectionStrings:{name}");
		}
	}
}