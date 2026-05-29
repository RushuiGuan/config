using Microsoft.Extensions.Configuration;
using Xunit;

namespace Albatross.Config.UnitTest {
	public class Extensions_BuildConfiguration {
		IConfigurationSection? Get(IConfiguration config, string path) {
			var keys = path.Split(':');
			IConfigurationSection? section = null;
			for (int i = 0; i < keys.Length; i++) {
				if (i == 0) {
					section = config.GetSection(keys[i]);
				} else {
					section = section?.GetSection(keys[i]);
				}
			}
			return section;
		}

		[Theory]
		[InlineData("")]
		[InlineData("program")]
		[InlineData("program:app")]
		[InlineData("single-value-config")]
		[InlineData("ConnectionStrings:configDatabaseConnection")]
		public void GetSection_ReturnsCorrectPath(string path) {
			var config = Extensions.BuildConfiguration();
			var section = Get(config, path);
			Assert.Equal(path, section?.Path);
		}

		[Theory]
		[InlineData("", null)]
		[InlineData("program", null)]
		[InlineData("program:app", "config-unittest")]
		[InlineData("ConnectionStrings:my-database", "azure-db")]
		public void GetSection_ReturnsCorrectValue(string path, string? expectedValue) {
			var config = Extensions.BuildConfiguration();
			Assert.Equal(expectedValue, config.GetSection(path)?.Value);
		}

		[Theory]
		[InlineData("", null)]
		[InlineData("program", null)]
		[InlineData("program:app", "config-unittest")]
		[InlineData("ConnectionStrings:my-database", "azure-db")]
		public void Indexer_ReturnsCorrectValue(string path, string? expectedValue) {
			var config = Extensions.BuildConfiguration();
			Assert.Equal(expectedValue, config[path]);
		}

		[Fact]
		public void EmptyKey_NeverReturnsNull() {
			var config = Extensions.BuildConfiguration();
			Assert.NotNull(config.GetSection(string.Empty));
		}
	}
}
