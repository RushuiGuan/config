using System.IO;
using Xunit;

namespace Albatross.Config.UnitTest {
	public class ApplicationPath_GetAbsolutePath {
		[Fact]
		public void AbsolutePath_ReturnedUnchanged() {
			var absolutePath = Path.Combine(Path.GetTempPath(), "myapp");
			var result = ApplicationPath.GetAbsolutePath(absolutePath);
			Assert.Equal(absolutePath, result);
		}

		[Fact]
		public void RelativePath_ResolvedAgainstCurrentDirectory() {
			var result = ApplicationPath.GetAbsolutePath("relative/path");
			var expected = Path.GetFullPath("relative/path");
			Assert.Equal(expected, result);
		}
	}
}
