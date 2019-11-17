using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Groomgy.MessageConsumer
{
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
}