using System;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer
{
    public class Message
    {
    }

    public class MessageMapper: IMapper<Message>
    {
        public Task<Message> Map(string raw)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageHandler : IHandler<Message>
    {
        public Task<bool> CanHandle(string raw)
        {
            throw new NotImplementedException();
        }

        public Task Handle(Message message)
        {
            throw new NotImplementedException();
        }
    }
}