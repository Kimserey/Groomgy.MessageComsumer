using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
    public interface IHost<TRaw>
    {
        IHost<TRaw> ConfigureServices(Action<IConfiguration, IServiceCollection> configureServices);

        IHost<TRaw> ConfigureLogger(Action<ILoggingBuilder> configureLogger);

        IHost<TRaw> Map<TPathFiler>(Action<IPathBuilder<TRaw>> builder) where TPathFiler: IPathFiler<TRaw>;

        void Start();
    }

    public interface IPathBuilder<TRaw>
    {
        IHost<TRaw> AddDecoder<TMessage, TMapper>()
            where TMapper: IDecoder<TRaw, TMessage>;

        IHost<TRaw> AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;
    }

    public interface IPathFiler<in TRaw>
    {
        Task<bool> Filter(TRaw message);
    }

    public interface IHandler<in TMessage>
    {
        Task<bool> CanHandle(Context context, TMessage message);

        Task<bool> Handle(Context context, TMessage message);
    }

    public interface IDecoder<in TRaw, TMessage>
    {
        Task<bool> CanDecode(Context context, TMessage message);

        Task<bool> Map(Context context, TRaw raw, out TMessage mapped);
    }

    public class Context: Dictionary<string ,string>
    {
        public string CorrelationId { get; set; }
    }
}