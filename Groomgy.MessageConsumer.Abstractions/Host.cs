using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer.Abstractions
{
    public class Host: IHost
    {
        private readonly IConsumer _consumer;
        
        private readonly IServiceCollection _services;
        private readonly IConfiguration  _configuration;

        private readonly List<(Type, MethodInfo, MethodInfo)> _handlerMethods = new List<(Type, MethodInfo, MethodInfo)>();
        private readonly List<(Type, MethodInfo)> _mapperMethods = new List<(Type, MethodInfo)>();

        public Host(IConsumer consumer)
        {
            _consumer = consumer;

            _services = new ServiceCollection();

            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        public IHost ConfigureLogger(Action<ILoggingBuilder> configureLogger)
        {
            _services.AddLogging(configureLogger);
            return this;
        }

        public IHost ConfigureServices(Action<IConfiguration, IServiceCollection> configure)
        {
            configure(_configuration, _services);
            return this;
        }

        public IHost AddHandler<TMessage, THandler>() where THandler : IHandler<TMessage>
        {
            _handlerMethods.Add((
                typeof(THandler),
                typeof(THandler).GetMethod("CanHandle"),
                typeof(THandler).GetMethod("Handle")
            ));
            return this;
        }

        public IHost AddMapper<TMessage, TMapper>() where TMapper : IMapper<TMessage>
        {
            _mapperMethods.Add((
                typeof(TMapper),
                typeof(TMapper).GetMethod("Map")
            ));
            return this;
        }

        public void Start()
        {
            var provider =
                _services.BuildServiceProvider();

            var context = 
                new Context { CorrelationId = Guid.NewGuid().ToString() };

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();

            _consumer.Consume(async raw =>
            {
                var isMapped = false;
                var handled = false;

                using (var scope = provider.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Host>>();
                    sw1.Restart();
                    logger.LogInformation(
                        "Starting consuming message. corId={corId}", context.CorrelationId
                    );

                    foreach (var (mapperType, mapFunc) in _mapperMethods)
                    {
                        var mapper =
                            ActivatorUtilities.CreateInstance(scope.ServiceProvider, mapperType);

                        var args = new object[3];
                        args[0] = context;
                        args[1] = raw;

                        sw2.Restart();
                        try
                        {
                            logger.LogInformation(
                                "Calling #{mapperType}.Map. corId={corId} raw={raw}", 
                                mapperType, context.CorrelationId, raw
                            );

                            isMapped = await (Task<bool>) mapFunc.Invoke(mapper, args);
                            
                            logger.LogInformation(
                                "Completed #{mapperType}.Map in {elapsedMs} ms. corId={corId} context={context}", 
                                mapperType, sw2.ElapsedMilliseconds, context.CorrelationId, context
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "A failure occurred while calling #{mapperType}.Map. corId={corId} context={context} " 
                                + "raw={raw} ex={ex}", 
                                mapperType, context.CorrelationId, context, raw, ex.Message);
                        }

                        if (!isMapped)
                            continue;

                        var mapped = args[2];

                        foreach (var (handlerType, canHandleFunc, handleFunc) in _handlerMethods)
                        {
                            var handler =
                                ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);

                            var canHandle = false;

                            try
                            {
                                sw2.Restart();
                                logger.LogInformation(
                                    "Start #{handlerType}.CanHandle. corId={corId} context={context}", 
                                    handlerType, context.CorrelationId, context
                                );

                                canHandle = await (Task<bool>) canHandleFunc.Invoke(handler, new[] {context, mapped});

                                logger.LogInformation(
                                    "Completed #{handlerType}.CanHandle in {elapsedMs} ms. corId={corId} context={context}", 
                                    handlerType, sw2.ElapsedMilliseconds, context.CorrelationId, context
                                );
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex,
                                    "A failure occurred during #{handlerType}.CanHandle. corId={corId} context={context} "
                                    + "mapped={mapped} ex={ex}",
                                    handlerType, context.CorrelationId, context, mapped, ex.Message);
                            }

                            if (!canHandle)
                                continue;

                            try
                            {
                                sw2.Restart();
                                logger.LogInformation(
                                    "Start #{handlerType}.Handle. corId={corId} type= context={context}", 
                                    handlerType, context.CorrelationId, context
                                );

                                handled = await (Task<bool>) handleFunc.Invoke(handler, new[] {context, mapped});

                                logger.LogInformation(
                                    "Completed #{handlerType}.Handle in {elapsedMs} ms. corId={corId} context={context}",
                                    handlerType, sw2.ElapsedMilliseconds, context.CorrelationId, context
                                );
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex,
                                    "A failure occurred during #{handlerType}.Handle. corId={corId} context={context} mapped={mapped} ex={ex}", 
                                    handlerType, context.CorrelationId, context, mapped, ex.Message);
                            }

                            if (handled)
                            {
                                logger.LogInformation(
                                    "Successfully consumed message in {elapsedMs} ms with #{mapperType}/#{handlerType}. corId={corId} context={context}",
                                    sw1.ElapsedMilliseconds, mapperType, handlerType, context.CorrelationId, context
                                    );
                                break;
                            }
                        }

                        if (handled)
                            break;
                    }

                    if (!handled)
                    {
                        logger.LogWarning(
                            "Failed to consume message and will be skipped. corId={corId} context={context} " 
                            + "elapsedMs={elapsedMs}",
                            context.CorrelationId, context, sw1.ElapsedMilliseconds
                        );
                    }
                }
            });
            
            provider.GetRequiredService<ILogger<Host>>().LogInformation(
                "Consumer successfully started."
            );
        }
    }
}