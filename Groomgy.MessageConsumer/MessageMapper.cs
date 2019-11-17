using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
    public class MessageDecoder: IDecoder<string, Message>
    {
        private readonly ILogger<MessageDecoder> _logger;

        public MessageDecoder(ILogger<MessageDecoder> logger)
        {
            _logger = logger;
        }

        public Task<bool> CanDecode(Context context, Message message)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Decode(Context context, string raw, out Message mapped)
        {
            context["label"] = "V1";
            context["another"] = "something else";
            mapped = JsonConvert.DeserializeObject<Message>(raw);
            return Task.FromResult(true);
        }
    }
}