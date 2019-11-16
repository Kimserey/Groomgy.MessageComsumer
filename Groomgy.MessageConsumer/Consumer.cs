using System;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer
{
    public interface IFakeConsumer
    {
        Task Consume(Action<string> consume);
    }

    public class FakeConsumer: IFakeConsumer
    {
        public Task Consume(Action<string> consume)
        {
            throw new NotImplementedException();
        }
    }
}