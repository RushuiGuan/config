using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Albatross.Config.UnitTest {
	public class ConfigBase_Validate {
		[Fact]
		public void MissingRequiredField_ThrowsValidationException() {
			var cfg = new ValidationTest(Extensions.BuildConfiguration());
			Assert.Throws<ValidationException>(() => cfg.Validate());
		}
	}
}
