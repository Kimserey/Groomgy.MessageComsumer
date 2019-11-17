using System;
using System.Collections.Generic;
using System.Reflection;
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

        IHost<TRaw> Map<TPathFilter>(Action<IPathBuilder<TRaw>> builder)
            where TPathFilter: IPathFilter<TRaw>;

        void Start();
    }

    public interface IPathHandler<in TRaw>
    {
        Task<bool> Handle(IServiceProvider services, Context context, TRaw message);
    }

    public interface IPathBuilder<TRaw>
    {
        IPathBuilder<TRaw> AddDecoder<TMessage, TMapper>()
            where TMapper: IDecoder<TRaw, TMessage>;

        IPathBuilder<TRaw> AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IPathHandler<TRaw> Build();
    }

    public interface IPathFilter<in TRaw>
    {
        Task<bool> Filter(TRaw message);
    }

    public interface IDecoder<in TRaw, TMessage>
    {
        Task<bool> CanDecode(Context context, TRaw raw);

        Task<bool> Decode(Context context, TRaw raw, out TMessage mapped);
    }

    public interface IHandler<in TMessage>
    {
        Task<bool> CanHandle(Context context, TMessage message);

        Task<bool> Handle(Context context, TMessage message);
    }

    public class Context : Dictionary<string, string>
    {
        public string CorrelationId { get; set; }
    }
}