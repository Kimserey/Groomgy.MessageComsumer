using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public class Hello
    {
        public string Text { get; set; }
    }

    public class HelloFilter : PathFilterBase<string>
    {
        private readonly ILogger<HelloFilter> _logger;

        public HelloFilter(ILogger<HelloFilter> logger)
        {
            _logger = logger;
        }

        public override Task<bool> Filter(string message)
        {
            _logger.LogInformation("Hello! corId={corId}", Context.CorrelationId);
            return Task.FromResult(message.Contains("Hello"));
        }
    }

    public class HelloDecoder : DecoderBase<string, Hello>
    {
        private readonly ILogger<HelloDecoder> _logger;

        public HelloDecoder(ILogger<HelloDecoder> logger)
        {
            _logger = logger;
        }

        public override Task<bool> CanDecode(string raw)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> Decode(string raw, out Hello mapped)
        {
            mapped = JsonConvert.DeserializeObject<Hello>(raw);
            _logger.LogInformation("Successfully mapped Hello world");
            return Task.FromResult(true);
        }
    }

    public class HelloHandler : HandlerBase<Hello>
    {
        private readonly ILogger<HelloHandler> _logger;

        public HelloHandler(ILogger<HelloHandler> logger)
        {
            _logger = logger;
        }

        public override Task<bool> CanHandle(Hello message)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> Handle(Hello mapped)
        {
            _logger.LogInformation("Handled {mapped}", mapped);
            return Task.FromResult(true);
        }
    }
}