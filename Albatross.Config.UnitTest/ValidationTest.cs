using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Albatross.Config.UnitTest {
	public class ValidationTest : ConfigBase {
		public ValidationTest(IConfiguration configuration) : base(configuration, "validation-test1") {
		}
		[Required]
		public string Name { get; } = null!;

		[Required]
		public string Data { get; } = null!;
	}
}