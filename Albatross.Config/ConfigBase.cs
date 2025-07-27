using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Albatross.Config {
	/// <summary>
	/// Base class for the configuration class.  
	/// </summary>
	public abstract class ConfigBase {
#if NET6_0_OR_GREATER		
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The concrete class is annotated with DynamicallyAccessedMembers(PublicProperties)")]
#endif
		protected ConfigBase(IConfiguration configuration, string? key) {
			if (!string.IsNullOrEmpty(key)) {
				var section = configuration.GetSection(key!);
				section.Bind(this);
			}
		}
#if NET6_0_OR_GREATER
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The concrete class is annotated with DynamicallyAccessedMembers(PublicProperties)")]
#endif
		public virtual void Validate() {
			Validator.ValidateObject(this, new ValidationContext(this));
		}
	}
}