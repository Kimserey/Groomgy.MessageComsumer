using System;
using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public class Message
    {
        public string Content { get; set; }
    }

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
            _logger.LogInformation("From Pathfilter: {name} {corId}", _service.GetName(), Context.CorrelationId);
            return Task.FromResult(!message.Contains("Hello"));
        }
    }

    public class MessageDecoder: DecoderBase<string, Message>
    {
        private readonly ILogger<MessageDecoder> _logger;
        private readonly INameService _service;

        public MessageDecoder(ILogger<MessageDecoder> logger, INameService service)
        {
            _logger = logger;
            _service = service;
        }

        public override Task<bool> CanDecode(string raw)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> Decode(string raw, out Message mapped)
        {
            Context["label"] = "V1";
            Context["another"] = "something else";
            mapped = JsonConvert.DeserializeObject<Message>(raw);

            _logger.LogInformation("From Decoder {name}", _service.GetName());
            return Task.FromResult(true);
        }
    }

    public class MessageHandler : HandlerBase<Message>
    {
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(ILogger<MessageHandler> logger)
        {
            _logger = logger;
        }

        public override Task<bool> CanHandle(Message message)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> Handle(Message message)
        {
            _logger.LogInformation("From handler {label}.", Context["label"]);
            return Task.FromResult(true);
        }
    }
}