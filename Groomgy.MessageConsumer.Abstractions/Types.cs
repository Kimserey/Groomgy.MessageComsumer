using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class Context: Dictionary<string ,string>
    {
        public string CorrelationId { get; set; }
    }

    public interface IConsumer
    {
        Task Consume(Action<string> consume);
    }

    public interface IHandler<in TMessage>
    {
        Task<bool> CanHandle(Context context, TMessage message);

        Task<bool> Handle(Context context, TMessage message);
    }

    public interface IMapper<TMessage>
    {
        Task<bool> Map(Context context, string raw, out TMessage mapped);
    }

    public interface IHost
    {
        IHost ConfigureServices(Action<IConfiguration, IServiceCollection> configureServices);

        IHost AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IHost AddMapper<TMessage, TMapper>()
            where TMapper: IMapper<TMessage>;

        void Start();
    }
}