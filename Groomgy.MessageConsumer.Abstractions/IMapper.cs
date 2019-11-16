using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IMapper<TMessage>
    {
        Task<bool> Map(Context context, string raw, out TMessage mapped);
    }
}