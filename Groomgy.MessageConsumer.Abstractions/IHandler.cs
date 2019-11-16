using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IHandler<in TMessage>
    {
        Task<bool> CanHandle(Context context, TMessage message);

        Task<bool> Handle(Context context, TMessage message);
    }
}