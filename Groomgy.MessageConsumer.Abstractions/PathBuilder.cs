using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
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

            public async Task<bool> Handle(IServiceProvider services, TRaw raw)
            {
                var handled = false;
                var sw = new Stopwatch();
                var logger = services.GetRequiredService<ILogger<Host<TRaw>>>();
                var context = services.GetRequiredService<Context>();

                logger.LogInformation(
                    "Starting handling message. corId={corId} context={context}", 
                    context.CorrelationId, context
                );

                foreach (var (decoderMeta, messageType) in _decoders)
                {
                    sw.Restart();

                    var decoder =
                        ActivatorUtilities.CreateInstance(services, decoderMeta.Type);

                    decoderMeta.Type
                        .GetProperty("Context")
                        ?.SetValue(decoder, context);

                    var canDecode = 
                        await (Task<bool>) decoderMeta.CanPerform.Invoke(
                            decoder,
                            new object[] {raw}
                        );

                    if (!canDecode)
                        // Skip current decoder as not suited to decode the message.
                        continue;

                    var args = new object[2];
                    args[0] = raw;

                    bool isDecoded;

                    try
                    {
                        isDecoded =
                            await (Task<bool>) decoderMeta.Perform.Invoke(
                                decoder,
                                args
                            );
                    }
                    catch (Exception ex)
                    {
                        throw new DecoderException(
                            $"Decoding failed for a suited decoder {decoderMeta.Type.Name}. Message={ex.Message}", 
                            decoderMeta.Type);
                    }

                    if (!isDecoded)
                        throw new DecoderException(
                            $"Decoding failed for a suited decoder {decoderMeta.Type.Name}.", 
                            decoderMeta.Type);

                    var decoded = args[1];

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

                        handlerMeta.Type
                            .GetProperty("Context")
                            ?.SetValue(handler, context);

                        var canHandle =
                            await (Task<bool>) handlerMeta.CanPerform.Invoke(
                                handler,
                                new [] {decoded}
                            );

                        if (!canHandle)
                            continue;

                        try
                        {
                            handled =
                                await (Task<bool>) handlerMeta.Perform.Invoke(
                                    handler,
                                    new[] {decoded}
                                );
                        }
                        catch (Exception ex)
                        {
                            throw new HandlerException(
                                $"Handling failed for suited handler {handlerMeta.Type.Name}. Message={ex.Message}", 
                                handlerMeta.Type);
                        }

                        if (!handled) 
                            throw new HandlerException(
                                $"Handling failed for suited handler {handlerMeta.Type.Name}.", 
                                handlerMeta.Type);

                        sw.Stop();
                        logger.LogInformation(
                            "Successfully handled message in {elapsedMs} ms with #{decoderType}/#{handlerType}. corId={corId} context={context}", 
                            sw.ElapsedMilliseconds, decoderMeta.Type, handlerMeta.Type, context.CorrelationId, context
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