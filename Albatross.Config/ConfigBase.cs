using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
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

		[Obsolete("Call the (IConfiguration, string?) constructor from derived types. This constructor will be removed in a future major version.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected ConfigBase(IConfiguration configuration) : this(configuration, null) {
			// do NOT bind here; we’ll bind from Key (if overridden)
			// If legacy subclasses still override Key, bind once using that value.
			if (!string.IsNullOrEmpty(Key)) {
				configuration.GetSection(Key).Bind(this);
			}
		}
		
#if NET6_0_OR_GREATER
		[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The concrete class is annotated with DynamicallyAccessedMembers(PublicProperties)")]
#endif
		public virtual void Validate() {
			Validator.ValidateObject(this, new ValidationContext(this));
		}

		[Obsolete("Override the (IConfiguration, string?) constructor instead. This property will be removed in a future major version.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual string Key => string.Empty;
	}
}