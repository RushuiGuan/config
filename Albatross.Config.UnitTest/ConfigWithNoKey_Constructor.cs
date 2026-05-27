using Xunit;

namespace Albatross.Config.UnitTest {
	public class ConfigWithNoKey_Constructor {
		[Fact]
		public void BindsConnectionStringAndEndpoint() {
			var cfg = new ConfigWithNoKey(Extensions.BuildConfiguration());
			Assert.Equal("azure-db", cfg.ConnectionString);
			Assert.Equal("microsoft.com/", cfg.EndPoint);
		}
	}
}
