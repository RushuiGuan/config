using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace AotTest {
	[Verb<RunSomething>("run-something")]
	public record RunSomethingOptions {
		[Option(DefaultToInitializer = true)]
		public string Name { get; init; } = string.Empty;
	}
	public class RunSomething : BaseHandler<RunSomethingOptions> {
		private readonly MyConfig config;
		private readonly ILogger<RunSomething> logger;

		public RunSomething(ParseResult result, RunSomethingOptions parameters, MyConfig config, ILogger<RunSomething> logger) : base(result, parameters) {
			this.config = config;
			this.logger = logger;
		}

		public override Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Hello {name}", config.Name);
			return Task.FromResult(0);
		}
	}
}
