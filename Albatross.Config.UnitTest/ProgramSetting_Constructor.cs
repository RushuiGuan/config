using Xunit;

namespace Albatross.Config.UnitTest {
	public class ProgramSetting_Constructor {
		[Fact]
		public void BindsFromAppSettings() {
			var setting = new ProgramSetting(Extensions.BuildConfiguration());
			Assert.Equal("config-unittest", setting.App);
			Assert.Equal("config", setting.Group);
			Assert.Equal("windows", setting.ServiceManager);
		}
	}
}
