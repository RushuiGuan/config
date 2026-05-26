using System.IO;
using Xunit;

namespace Albatross.Config.UnitTest {
	public class ApplicationPath_Constructor {
		private readonly string[] subFolders = ["mycompany", "myapp"];
		private const string sectionKey = "myapp";

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void IsSystemPath_ReflectsConstructorArg(bool useSystemPath) {
			var path = new ApplicationPath(useSystemPath, subFolders, sectionKey, []);
			Assert.Equal(useSystemPath, path.IsSystemPath);
		}

		[Fact]
		public void NoOverrides_UserPath_DefaultsUnderUserRoot() {
			var path = new ApplicationPath(false, subFolders, sectionKey, []);
			var root = ApplicationPath.GetUserRootPath();
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "data"), path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}

		[Fact]
		public void NoOverrides_SystemPath_DefaultsUnderSystemRoot() {
			var path = new ApplicationPath(true, subFolders, sectionKey, []);
			var root = ApplicationPath.GetSystemRootPath();
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "data"), path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}

		[Fact]
		public void CommandLine_DataRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			var path = new ApplicationPath(false, subFolders, sectionKey, [$"--{sectionKey}:dataRoot={overridePath}"]);
			Assert.Equal(overridePath, path.DataRoot);
		}

		[Fact]
		public void CommandLine_ConfigRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-config");
			var path = new ApplicationPath(false, subFolders, sectionKey, [$"--{sectionKey}:configRoot={overridePath}"]);
			Assert.Equal(overridePath, path.ConfigRoot);
		}

		[Fact]
		public void CommandLine_LogRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-log");
			var path = new ApplicationPath(false, subFolders, sectionKey, [$"--{sectionKey}:logRoot={overridePath}"]);
			Assert.Equal(overridePath, path.LogRoot);
		}

		[Fact]
		public void CommandLine_RelativeDataRoot_ResolvedAgainstCurrentDirectory() {
			var path = new ApplicationPath(false, subFolders, sectionKey, [$"--{sectionKey}:dataRoot=relative/data"]);
			var expected = Path.GetFullPath("relative/data");
			Assert.Equal(expected, path.DataRoot);
		}

		[Fact]
		public void CommandLine_OverrideOneRoot_OthersRemainDefault() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			var path = new ApplicationPath(false, subFolders, sectionKey, [$"--{sectionKey}:dataRoot={overridePath}"]);
			var root = ApplicationPath.GetUserRootPath();
			Assert.Equal(overridePath, path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}
	}
}
