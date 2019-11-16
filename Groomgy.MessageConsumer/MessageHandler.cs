using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
    public class MessageHandler : IHandler<Message>
    {
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(ILogger<MessageHandler> logger)
        {
            _logger = logger;
        }

        public Task<bool> CanHandle(Context context, Message message)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Handle(Context context, Message message)
        {
            return Task.FromResult(true);
        }
    }
}