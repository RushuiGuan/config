using Microsoft.Extensions.Configuration;

namespace Albatross.Config.UnitTest {
	public class SingleValueConfig : ConfigBase {
		public string? Value { get; set; }
		public SingleValueConfig(IConfiguration configuration) : base(configuration, null) {
			this.Value = configuration.GetSection("single-value-config").Get<string>();
		}
	}
}