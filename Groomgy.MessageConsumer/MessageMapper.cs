using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public class MessageMapper: IMapper<Message>
    {
        private readonly ILogger<MessageMapper> _logger;

        public MessageMapper(ILogger<MessageMapper> logger)
        {
            _logger = logger;
        }
    
        public Task<bool> Map(Context context, string raw, out Message mapped)
        {
            context["label"] = "V1";
            context["another"] = "something else";
            mapped = JsonConvert.DeserializeObject<Message>(raw);
            return Task.FromResult(true);
        }
    }
}