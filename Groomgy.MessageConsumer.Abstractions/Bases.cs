using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{    
    /// <summary>
    /// Base path filter with a context.
    /// </summary>
    /// <typeparam name="TRaw">Raw type message.</typeparam>
    public abstract class PathFilterBase<TRaw> : IPathFilter<TRaw>
    {
        public abstract Task<bool> Filter(TRaw message);

        public Context Context { get; set; }
    }

    /// <summary>
    /// Base decoder with a context.
    /// </summary>
    /// <typeparam name="TRaw">Raw type message.</typeparam>
    /// <typeparam name="TMessage">Decoded type message.</typeparam>
    public abstract class DecoderBase<TRaw, TMessage> : IDecoder<TRaw, TMessage>
    {
        public abstract Task<bool> CanDecode(TRaw raw);

        public abstract Task<bool> Decode(TRaw raw, out TMessage mapped);

        public Context Context { get; set; }
    }

    /// <summary>
    /// Base handler with a context.
    /// </summary>
    /// <typeparam name="TMessage">Decoded type message.</typeparam>
    public abstract class HandlerBase<TMessage> : IHandler<TMessage>
    {
        public abstract Task<bool> CanHandle(TMessage message);

        public abstract Task<bool> Handle(TMessage mapped);

        public Context Context { get; set; }
    }
}