using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Groomgy.MessageConsumer.Abstractions
{
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
            return new PathHandler(
                _decoderMethods.ToArray(),
                _handlerMethods.ToArray()
            );
        }

        public class PathHandler: IPathHandler<TRaw>
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
                foreach (var decoder in _decoders)
                {
                    //decode and find the handler that handle this type based on the underlying type
                }
            }
        }

        public class Meta
        {
            public Type Type { get; set; }

            public MethodInfo CanPerform { get; set; }

            public MethodInfo Perform { get; set; }
        }
    }
}