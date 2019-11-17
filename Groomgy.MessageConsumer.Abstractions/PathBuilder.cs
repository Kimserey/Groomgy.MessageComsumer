using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class NoHandlerException : Exception
    {
        public NoHandlerException(string message) : base(message) { }
    }

    public class PathBuilder<TRaw> : IPathBuilder<TRaw>
    {
        private readonly List<(Meta, Type)> _decoderMethods = 
            new List<(Meta, Type)>();
        private readonly Dictionary<Type, List<Meta>> _handlerMethods = 
            new Dictionary<Type, List<Meta>>();

        public IPathBuilder<TRaw> AddDecoder<TMessage, TDecoder>()
            where TDecoder : IDecoder<TRaw, TMessage>
        {
            _decoderMethods.Add((
                new Meta {
                    Type = typeof(TDecoder),
                    CanPerform = typeof(TDecoder).GetMethod("CanDecode"),
                    Perform = typeof(TDecoder).GetMethod("Decode")
                },
                typeof(TMessage)
            ));
            return this;
        }

        public IPathBuilder<TRaw> AddHandler<TMessage, THandler>()
            where THandler : IHandler<TMessage>
        {
            if (!_handlerMethods.ContainsKey(typeof(TMessage)))
                _handlerMethods[typeof(TMessage)] = new List<Meta>();

            _handlerMethods[typeof(TMessage)].Add(new Meta {
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
                _handlerMethods.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray())
            );
        }

        public class PathHandler: IPathHandler<TRaw>
        {
            private readonly (Meta, Type)[] _decoders;
            private readonly Dictionary<Type, Meta[]> _handlers;

            public PathHandler((Meta, Type)[] decoders, Dictionary<Type, Meta[]> handlers)
            {
                _decoders = decoders;
                _handlers = handlers;
            }

            public async Task<bool> Handle(IServiceProvider services, Context context, TRaw raw)
            {
                var handled = false;

                foreach (var (decoderMeta, messageType) in _decoders)
                {
                    var decoder =
                        ActivatorUtilities.CreateInstance(services, decoderMeta.Type);

                    var canDecode =
                        await (Task<bool>) decoderMeta.CanPerform.Invoke(
                            decoder,
                            new object[]{ context, raw }
                        );

                    if (!canDecode)
                        continue;

                    var args = new object[3];
                    args[0] = context;
                    args[1] = raw;

                    var isDecoded =
                        await (Task<bool>) decoderMeta.Perform.Invoke(
                            decoder,
                            args
                        );

                    if (!isDecoded)
                        break;

                    var decoded = args[2];

                    if (!_handlers.ContainsKey(messageType))
                    {
                        throw new NoHandlerException(
                            "No handlers registered to handle the current message. " +
                            "Verify that you have a handler for the message. Or verfiy that " +
                            "your `PathFilter` is configured to map to the right hanlders.");
                    }

                    foreach (var handlerMeta in _handlers[messageType])
                    {
                        var handler =
                            ActivatorUtilities.CreateInstance(services, handlerMeta.Type);

                        var canHandle =
                            await (Task<bool>) handlerMeta.CanPerform.Invoke(
                                handler,
                                new [] {context, decoded}
                            );

                        if (!canHandle)
                            continue;

                        handled =
                            await (Task<bool>) handlerMeta.Perform.Invoke(
                                handler,
                                new[] {context, decoded}
                            );
                    }

                    // Handling has been completed by now,
                    // therefore we break out of the loop.
                    break;
                }

                return handled;
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