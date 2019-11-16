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

    public class MessageMapper: IMapper<Message>
    {
        private readonly ILogger<MessageMapper> _logger;

        public MessageMapper(ILogger<MessageMapper> logger)
        {
            _logger = logger;
        }
    
        public Task<bool> Map(Context context, string raw, out Message mapped)
        {
            _logger.LogInformation("Start mapping. corId={corId}", context.CorrelationId);
            context["label"] = "V1";

            mapped = JsonConvert.DeserializeObject<Message>(raw);
            _logger.LogInformation("Completed mapping. corId={corId}", context.CorrelationId);
            return Task.FromResult(true);
        }
    }

    public class MessageHandler : IHandler<Message>
    {
        private readonly ILogger<MessageMapper> _logger;

        public MessageHandler(ILogger<MessageMapper> logger)
        {
            _logger = logger;
        }

        public Task<bool> CanHandle(Context context, Message message)
        {
            _logger.LogInformation(
                "Start can handle. corId={corId} label={label}", 
                context.CorrelationId, context["label"]
            );

            _logger.LogInformation(
                "Completed can handle. corId={corId} label={label}", 
                context.CorrelationId, context["label"]
            );
            return Task.FromResult(true);
        }

        public Task<bool> Handle(Context context, Message message)
        {
            _logger.LogInformation(
                "Start handle. corId={corId} label={label}", 
                context.CorrelationId, context["label"]
            );

            _logger.LogInformation(
                "Completed handle. corId={corId} label={label}", 
                context.CorrelationId, context["label"]
            );
            return Task.FromResult(true);
        }
    }
}