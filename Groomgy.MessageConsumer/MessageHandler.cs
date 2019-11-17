using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
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