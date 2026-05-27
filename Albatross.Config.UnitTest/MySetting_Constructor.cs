using Xunit;

namespace Albatross.Config.UnitTest {
	public class MySetting_Constructor {
		[Fact]
		public void BindsNestedObjectFromAppSettings() {
			var config = new MySetting(Extensions.BuildConfiguration());
			Assert.Equal("my test data", config.Name);
			Assert.NotNull(config.Data);
			Assert.Equal(100, config.Data?.Count);
		}
	}
}
