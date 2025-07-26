using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Albatross.Config.UnitTest {
	public class DbConfig : ConfigBase {
		public DbConfig(IConfiguration configuration) : base(configuration, "db-config") {
		}
		public string? Data { get; set; }
	}
}