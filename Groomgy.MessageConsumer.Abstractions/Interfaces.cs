using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
    /// <summary>
    /// An interface for context'd objects.
    /// </summary>
    public interface IContext
    {
        Context Context { get; set; }
    }

    public class Context : Dictionary<string, string>
    {
        public string CorrelationId { get; set; }

        public Context()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }
    }

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
        Task<bool> Handle(IServiceProvider services, TRaw message);
    }

    public interface IPathBuilder<TRaw>
    {
        IPathBuilder<TRaw> AddDecoder<TMessage, TMapper>()
            where TMapper: IDecoder<TRaw, TMessage>;

        IPathBuilder<TRaw> AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IPathHandler<TRaw> Build();
    }

    public interface IPathFilter<in TRaw> : IContext
    {
        Task<bool> Filter(TRaw message);
    }

    public abstract class PathFilterBase<TRaw> : IPathFilter<TRaw>
    {
        public abstract Task<bool> Filter(TRaw message);

        public Context Context { get; set; }
    }

    public interface IDecoder<in TRaw, TMessage>: IContext
    {
        Task<bool> CanDecode(TRaw raw);

        Task<bool> Decode(TRaw raw, out TMessage mapped);
    }

    public abstract class DecoderBase<TRaw, TMessage> : IDecoder<TRaw, TMessage>
    {
        public abstract Task<bool> CanDecode(TRaw raw);

        public abstract Task<bool> Decode(TRaw raw, out TMessage mapped);

        public Context Context { get; set; }
    }

    public interface IHandler<in TMessage>: IContext
    {
        Task<bool> CanHandle(TMessage message);

        Task<bool> Handle(TMessage message);
    }

    public abstract class HandlerBase<TMessage> : IHandler<TMessage>
    {
        public abstract Task<bool> CanHandle(TMessage message);

        public abstract Task<bool> Handle(TMessage mapped);

        public Context Context { get; set; }
    }
}