using System;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IConsumer
    {
        Task Consume(Action<string> consume);
    }
}