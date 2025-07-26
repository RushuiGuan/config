using Microsoft.Extensions.Configuration;

namespace Albatross.Config.UnitTest {
	public class ChangeTest : ConfigBase {
		public ChangeTest(IConfiguration configuration) : base(configuration, "change-test") {
		}
	}
}