using Albatross.Config;
using Microsoft.Extensions.Configuration;

namespace AotTest {
	public class MyConfig : ConfigBase {
		public MyConfig(IConfiguration configuration) : base(configuration, "my") { }
		public string Name { get; set; } = string.Empty;
	}
}