using Albatross.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.CommandLine.Invocation;

namespace AotTest {
	[Verb("run-something", typeof(RunSomething))]
	public record RunSomethingOptions {
		public string Name { get; init; } = string.Empty;
	}
	public class RunSomething : BaseHandler<RunSomethingOptions> {
		private readonly MyConfig config;
		private readonly ILogger logger;

		public RunSomething(MyConfig config, IOptions<RunSomethingOptions> options, ILogger logger) : base(options) {
			this.config = config;
			this.logger = logger;
		}

		public override int Invoke(InvocationContext context) {
			logger.LogInformation($"Hello {config.Name}");
			return base.Invoke(context);
		}
	}
}
