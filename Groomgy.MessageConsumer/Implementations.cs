using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer
{
    public class Context: Dictionary<string ,string>
    {
        public string CorrelationId { get; set; }
    }

    public class Message
    {
    }

    public class MessageMapper: IMapper<Message>
    {
        public Task<bool> Map(Context context, string raw, out Message mapped)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageHandler : IHandler<Message>
    {
        public Task<bool> CanHandle(Context context, Message message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Handle(Context context, Message message)
        {
            throw new NotImplementedException();
        }
    }
}