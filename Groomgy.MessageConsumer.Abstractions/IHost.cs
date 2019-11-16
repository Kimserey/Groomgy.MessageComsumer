using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IHost
    {
        IHost ConfigureServices(Action<IConfiguration, IServiceCollection> configureServices);

        IHost ConfigureLogger(Action<ILoggingBuilder> configureLogger);

        IHost AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IHost AddMapper<TMessage, TMapper>()
            where TMapper: IMapper<TMessage>;

        void Start();
    }
}