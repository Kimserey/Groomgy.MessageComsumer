using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
    public class PathFilter: PathFilterBase<string>
    {
        private readonly INameService _service;
        private readonly ILogger<PathFilter> _logger;

        public PathFilter(INameService service, ILogger<PathFilter> logger)
        {
            _service = service;
            _logger = logger;
        }

        public override Task<bool> Filter(string message)
        {
            _logger.LogInformation("From Pathfilter: {name}", _service.GetName());
            return Task.FromResult(true);
        }
    }
}