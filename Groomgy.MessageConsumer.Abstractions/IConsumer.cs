using System;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IConsumer<out TRaw>
    {
        Task Consume(Action<TRaw> consume);
    }
}