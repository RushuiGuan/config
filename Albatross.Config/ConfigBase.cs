using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Albatross.Config {
	/// <summary>
	/// Base class for the configuration class.  
	/// </summary>
	public abstract class ConfigBase {
		protected ConfigBase(IConfiguration configuration, string? key) {
			if (!string.IsNullOrEmpty(key)) {
				var section = configuration.GetSection(key!);
				section.Bind(this);
			}
		}

		public virtual void Validate() {
			Validator.ValidateObject(this, new ValidationContext(this));
		}
	}
}