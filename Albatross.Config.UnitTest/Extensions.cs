using Microsoft.Extensions.Configuration;
using System;

namespace Albatross.Config.UnitTest {
	public static class Extensions {
		public static IConfiguration BuildConfiguration() {
			return new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", true, false)
				.Build();
		}
	}
}