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

    /// <summary>
    /// Context object containing a correlation Id.
    /// </summary>
    public class Context : Dictionary<string, string>
    {
        public string CorrelationId { get; set; }

        public Context()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// A host builder used to configure services, logger and
    /// routing paths to handle messages.
    /// </summary>
    /// <typeparam name="TRaw">Raw message type.</typeparam>
    public interface IHost<TRaw>
    {
        IHost<TRaw> ConfigureServices(Action<IConfiguration, IServiceCollection> configureServices);

        IHost<TRaw> ConfigureLogger(Action<ILoggingBuilder> configureLogger);

        /// <summary>
        /// Determines which path handler to consider.
        /// `TPathFilter` will be invoked and when `true`, decoders and handlers
        /// setup with the `IPathBuilder` will be considered.
        /// </summary>
        IHost<TRaw> Map<TPathFilter>(Action<IPathBuilder<TRaw>> builder)
            where TPathFilter: IPathFilter<TRaw>;

        void Start();
    }

    /// <summary>
    /// Handle a message for a specific path routed from the
    /// host.
    /// </summary>
    /// <typeparam name="TRaw">Raw message type.</typeparam>
    public interface IPathHandler<in TRaw>
    {
        Task<bool> Handle(IServiceProvider services, TRaw message);
    }

    /// <summary>
    /// Path builder used to construct a logic path to decode
    /// a message.
    /// </summary>
    /// <typeparam name="TRaw">Raw message type.</typeparam>
    public interface IPathBuilder<TRaw>
    {
        /// <summary>
        /// Adds a decoder to decode the raw message.
        /// Decoders are taken into account in order of
        /// registration and when successful, will determine
        /// which handlers to take into account.
        /// </summary>
        IPathBuilder<TRaw> AddDecoder<TMessage, TMapper>()
            where TMapper: IDecoder<TRaw, TMessage>;

        /// <summary>
        /// Adds a handler to handle the decoded message.
        /// </summary>
        IPathBuilder<TRaw> AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>;

        IPathHandler<TRaw> Build();
    }

    /// <summary>
    /// Path filter used to filter messages to decide which
    /// path handling to take.
    /// </summary>
    /// <typeparam name="TRaw">Raw type message.</typeparam>
    public interface IPathFilter<in TRaw> : IContext
    {
        /// <summary>
        /// Filters the message.
        /// To be used with #{Host}.Map.
        /// 
        /// Determines which decoders and handlers to take
        /// into account.
        /// </summary>
        Task<bool> Filter(TRaw message);
    }

    /// <summary>
    /// Decoder used to decode raw message to domain messages.
    /// </summary>
    /// <typeparam name="TRaw">Raw type message.</typeparam>
    /// <typeparam name="TMessage">Decoded type message.</typeparam>
    public interface IDecoder<in TRaw, TMessage>: IContext
    {
        /// <summary>
        /// Defines whether the decoder is suited to decode the
        /// message.
        /// When false, will move to the next registered decoder.
        /// </summary>
        Task<bool> CanDecode(TRaw raw);

        /// <summary>
        /// Decodes the message when `CanDecode` is true.
        /// Any suited decoder is expected to always decode the
        /// message succesfully. If decoding fails or return false,
        /// a `DecoderException` is thrown.
        ///
        /// Once decoded, only handlers of the `TMessage` will be
        /// considered.
        /// And if no handlers are registered, throw a `NoHandlerException`.
        /// </summary>
        Task<bool> Decode(TRaw raw, out TMessage mapped);
    }

    /// <summary>
    /// Handler used to handle decoded message.
    /// </summary>
    /// <typeparam name="TMessage">Decoded type message.</typeparam>
    public interface IHandler<in TMessage>: IContext
    {
        /// <summary>
        /// Defines whether the handler is suited to handle the
        /// message.
        /// When false, will move to the next registered handler.
        /// </summary>
        Task<bool> CanHandle(TMessage message);

        /// <summary>
        /// Handles the message when `CanHandle` is true.
        /// Any suited handler is expected to always handle the
        /// message succesfully. If handle fails or return false,
        /// a `HandlerException` is thrown.
        /// </summary>
        Task<bool> Handle(TMessage message);
    }
}