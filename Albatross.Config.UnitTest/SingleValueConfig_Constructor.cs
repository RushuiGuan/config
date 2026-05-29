using Xunit;

namespace Albatross.Config.UnitTest {
	public class SingleValueConfig_Constructor {
		[Fact]
		public void BindsFromAppSettings() {
			var value = new SingleValueConfig(Extensions.BuildConfiguration());
			Assert.Equal("www.google.com", value.Value);
		}
	}
}
