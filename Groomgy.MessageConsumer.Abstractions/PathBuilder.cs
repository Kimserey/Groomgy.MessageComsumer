using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class PathHandler<TRaw>: IPathHandler<TRaw>
    {
        private readonly Meta[] _decoders;
        private readonly Meta[] _handlers;

        public PathHandler(Meta[] decoders, Meta[] handlers)
        {
            _decoders = decoders;
            _handlers = handlers;
        }

        public Task<bool> Handle(TRaw message)
        {
            throw new NotImplementedException();
        }
    }

    public class PathBuilder<TRaw> : IPathBuilder<TRaw>
    {
        private readonly List<Meta> _decoderMethods = 
            new List<Meta>();
        private readonly List<Meta> _handlerMethods = 
            new List<Meta>();

        public IPathBuilder<TRaw> AddDecoder<TMessage, TDecoder>() where TDecoder : IDecoder<TRaw, TMessage>
        {
            _decoderMethods.Add(new Meta {
                Type = typeof(TDecoder),
                CanPerform = typeof(TDecoder).GetMethod("CanDecode"),
                Perform = typeof(TDecoder).GetMethod("Decode")
            });
            return this;
        }

        public IPathBuilder<TRaw> AddHandler<TMessage, THandler>() where THandler : IHandler<TMessage>
        {
            _handlerMethods.Add(new Meta {
                Type = typeof(THandler),
                CanPerform = typeof(THandler).GetMethod("CanHandle"),
                Perform = typeof(THandler).GetMethod("Handle")
            });
            return this;
        }

        public IPathHandler<TRaw> Build()
        {
            return new PathHandler<TRaw>(
                _decoderMethods.ToArray(),
                _handlerMethods.ToArray()
            );
        }
    }
}