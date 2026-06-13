using System;
using System.IO;
using Xunit;

namespace Albatross.Config.UnitTest {
	public class ApplicationPath_Constructor {
		private readonly string[] subFolders = ["mycompany", "myapp"];
		private const string sectionKey = "myapp";
		private const string subSectionKey = "paths";

		private void ClearEnvOverrides() {
			Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", null);
			Environment.SetEnvironmentVariable($"{sectionKey}__configRoot", null);
			Environment.SetEnvironmentVariable($"{sectionKey}__logRoot", null);
			Environment.SetEnvironmentVariable($"{sectionKey}__{subSectionKey}__dataRoot", null);
			Environment.SetEnvironmentVariable($"{sectionKey}__{subSectionKey}__configRoot", null);
			Environment.SetEnvironmentVariable($"{sectionKey}__{subSectionKey}__logRoot", null);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void IsSystemPath_ReflectsConstructorArg(bool useSystemPath) {
			var path = new ApplicationPath(useSystemPath, subFolders, sectionKey, null, []);
			Assert.Equal(useSystemPath, path.IsSystemPath);
		}

		[Theory]
		[InlineData(null, true)]    // userMode absent -> system path is the default
		[InlineData("true", false)] // userMode=true -> user path
		[InlineData("false", true)] // userMode=false -> system path
		public void UserMode_TogglesSystemPath(string? userMode, bool expectedSystemPath) {
			ClearEnvOverrides();
			string[] args = userMode is null ? [] : [$"--{sectionKey}:userMode={userMode}"];
			var path = new ApplicationPath(subFolders, sectionKey, null, args);
			Assert.Equal(expectedSystemPath, path.IsSystemPath);
		}

		[Fact]
		public void NoOverrides_UserPath_DefaultsUnderUserRoot() {
			var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
			var root = ApplicationPath.GetUserRootPath();
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "data"), path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}

		[Fact]
		public void NoOverrides_SystemPath_DefaultsUnderSystemRoot() {
			var path = new ApplicationPath(true, subFolders, sectionKey, null, []);
			var root = ApplicationPath.GetSystemRootPath();
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "data"), path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}

		[Fact]
		public void CommandLine_DataRootOverride_UsedAsIs() {
			ClearEnvOverrides();
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			var path = new ApplicationPath(false, subFolders, sectionKey, null, [$"--{sectionKey}:dataRoot={overridePath}"]);
			Assert.Equal(overridePath, path.DataRoot);
		}

		[Fact]
		public void CommandLine_ConfigRootOverride_UsedAsIs() {
			ClearEnvOverrides();
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-config");
			var path = new ApplicationPath(false, subFolders, sectionKey, null, [$"--{sectionKey}:configRoot={overridePath}"]);
			Assert.Equal(overridePath, path.ConfigRoot);
		}

		[Fact]
		public void CommandLine_LogRootOverride_UsedAsIs() {
			ClearEnvOverrides();
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-log");
			var path = new ApplicationPath(false, subFolders, sectionKey, null, [$"--{sectionKey}:logRoot={overridePath}"]);
			Assert.Equal(overridePath, path.LogRoot);
		}

		[Fact]
		public void CommandLine_RelativeDataRoot_ResolvedAgainstCurrentDirectory() {
			ClearEnvOverrides();
			var path = new ApplicationPath(false, subFolders, sectionKey, null, [$"--{sectionKey}:dataRoot=relative/data"]);
			var expected = Path.GetFullPath("relative/data");
			Assert.Equal(expected, path.DataRoot);
		}

		[Fact]
		public void CommandLine_OverrideOneRoot_OthersRemainDefault() {
			ClearEnvOverrides();
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			var path = new ApplicationPath(false, subFolders, sectionKey, null, [$"--{sectionKey}:dataRoot={overridePath}"]);
			var root = ApplicationPath.GetUserRootPath();
			Assert.Equal(overridePath, path.DataRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
		}

		[Fact]
		public void Environment_DataRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", overridePath);
				var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
				Assert.Equal(overridePath, path.DataRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", null);
			}
		}

		[Fact]
		public void Environment_ConfigRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-config");
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__configRoot", overridePath);
				var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
				Assert.Equal(overridePath, path.ConfigRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__configRoot", null);
			}
		}

		[Fact]
		public void Environment_LogRootOverride_UsedAsIs() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-log");
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__logRoot", overridePath);
				var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
				Assert.Equal(overridePath, path.LogRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__logRoot", null);
			}
		}

		[Fact]
		public void Environment_OverrideOneRoot_OthersRemainDefault() {
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", overridePath);
				var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
				var root = ApplicationPath.GetUserRootPath();
				Assert.Equal(overridePath, path.DataRoot);
				Assert.Equal(Path.Join(root, "mycompany", "myapp", "config"), path.ConfigRoot);
				Assert.Equal(Path.Join(root, "mycompany", "myapp", "log"), path.LogRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", null);
			}
		}

		[Fact]
		public void Environment_RelativeDataRoot_ResolvedAgainstCurrentDirectory() {
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", "relative/data");
				var path = new ApplicationPath(false, subFolders, sectionKey, null, []);
				var expected = Path.GetFullPath("relative/data");
				Assert.Equal(expected, path.DataRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__dataRoot", null);
			}
		}

		[Fact]
		public void SubSection_CommandLine_OverridesResolved() {
			ClearEnvOverrides();
			var dataRoot = Path.Combine(Path.GetTempPath(), "myapp-data");
			var configRoot = Path.Combine(Path.GetTempPath(), "myapp-config");
			var logRoot = Path.Combine(Path.GetTempPath(), "myapp-log");
			var path = new ApplicationPath(false, subFolders, sectionKey, subSectionKey, [
				$"--{sectionKey}:{subSectionKey}:dataRoot={dataRoot}",
				$"--{sectionKey}:{subSectionKey}:configRoot={configRoot}",
				$"--{sectionKey}:{subSectionKey}:logRoot={logRoot}",
			]);
			Assert.Equal(dataRoot, path.DataRoot);
			Assert.Equal(configRoot, path.ConfigRoot);
			Assert.Equal(logRoot, path.LogRoot);
		}

		[Fact]
		public void SubSection_Environment_OverridesResolved() {
			var dataRoot = Path.Combine(Path.GetTempPath(), "myapp-data");
			try {
				Environment.SetEnvironmentVariable($"{sectionKey}__{subSectionKey}__dataRoot", dataRoot);
				var path = new ApplicationPath(false, subFolders, sectionKey, subSectionKey, []);
				Assert.Equal(dataRoot, path.DataRoot);
			} finally {
				Environment.SetEnvironmentVariable($"{sectionKey}__{subSectionKey}__dataRoot", null);
			}
		}

		[Fact]
		public void SubSection_NonNestedOverride_Ignored() {
			ClearEnvOverrides();
			// When a sub-section is in effect, an override placed directly under the section is not picked up.
			var overridePath = Path.Combine(Path.GetTempPath(), "myapp-data");
			var path = new ApplicationPath(false, subFolders, sectionKey, subSectionKey, [$"--{sectionKey}:dataRoot={overridePath}"]);
			var root = ApplicationPath.GetUserRootPath();
			Assert.Equal(Path.Join(root, "mycompany", "myapp", "data"), path.DataRoot);
		}

		[Fact]
		public void SubSection_UserMode_ReadFromSectionNotSubSection() {
			ClearEnvOverrides();
			// userMode is always read from {sectionKey}:userMode, regardless of the sub-section used for path overrides.
			var path = new ApplicationPath(subFolders, sectionKey, subSectionKey, [$"--{sectionKey}:userMode=true"]);
			Assert.False(path.IsSystemPath);
		}

		[Fact]
		public void SubSection_UserModeUnderSubSection_Ignored() {
			ClearEnvOverrides();
			// userMode placed under the sub-section is not honored; the default (system path) remains in effect.
			var path = new ApplicationPath(subFolders, sectionKey, subSectionKey, [$"--{sectionKey}:{subSectionKey}:userMode=true"]);
			Assert.True(path.IsSystemPath);
		}
	}
}