using Xunit;

namespace Albatross.Config.UnitTest {
	public class DbConfig_Constructor {
		[Fact]
		public void MissingSection_DataIsNull() {
			var cfg = new DbConfig(Extensions.BuildConfiguration());
			Assert.Null(cfg.Data);
		}
	}
}
