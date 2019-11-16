using System;
using System.Threading.Tasks;
using Groomgy.MessageConsumer.Abstractions;

namespace Groomgy.MessageConsumer
{
    public class FakeConsumer: IConsumer
    {
        public Task Consume(Action<string> consume)
        {
            throw new NotImplementedException();
        }
    }
}