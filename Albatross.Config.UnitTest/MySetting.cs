using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Albatross.Config.UnitTest {
	public class GameData {
		public int Count { get; set; }
	}

	public class MySetting : ConfigBase {
		public string? Name { get; set; }
		public GameData? Data { get; set; }

		public MySetting(IConfiguration configuration) : base(configuration, "my") {
		}
	}
}