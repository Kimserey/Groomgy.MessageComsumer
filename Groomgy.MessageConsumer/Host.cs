using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Groomgy.MessageConsumer
{
    public class Host: IHost
    {
        private readonly IFakeConsumer _consumer;
        
        private readonly IServiceCollection _services;
        private readonly IConfiguration  _configuration;

        private readonly List<(Type, MethodInfo, MethodInfo)> _handlerMethods = new List<(Type, MethodInfo, MethodInfo)>();
        private readonly List<(Type, MethodInfo)> _mapperMethods = new List<(Type, MethodInfo)>();

        public Host(IFakeConsumer consumer)
        {
            _consumer = consumer;
            _services = new ServiceCollection();
            _configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
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
                new Context { CorrelationId = new Guid().ToString() };

            _consumer.Consume(async raw =>
            {
                var sw = Stopwatch.StartNew();
                var isMapped = false;
                var handled = false;

                using (var scope = provider.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHost>>();

                    logger.LogInformation(
                        "Starting consuming message. context={context}", context
                    );

                    foreach (var (mapperType, mapFunc) in _mapperMethods)
                    {
                        var mapper =
                            ActivatorUtilities.CreateInstance(scope.ServiceProvider, mapperType);

                        var args = new object[3];
                        args[0] = context;
                        args[1] = raw;

                        try
                        {
                            isMapped = await (Task<bool>) mapFunc.Invoke(mapper, args);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex,
                                "A failure occurred during mapping. context={context} raw={raw} ex={ex}", 
                                context, raw, ex.Message);
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
                                canHandle = await (Task<bool>) canHandleFunc.Invoke(handler, new[] {context, mapped});
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex,
                                    "A failure occurred during canHandle test. context={context} mapped={mapped} ex={ex}",
                                    context, mapped, ex.Message);
                            }

                            if (!canHandle)
                                continue;

                            try
                            {
                                handled = await (Task<bool>) handleFunc.Invoke(handler, new[] {context, mapped});
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex,
                                    "A failure occurred during handle. context={context} mapped={mapped} ex={ex}", 
                                    context, mapped, ex.Message);
                            }

                            if (handled)
                            {
                                logger.LogInformation(
                                "Completed consuming message. context={context} elapsedMs={elapsedMs}",
                                context, sw.ElapsedMilliseconds
                                );
                                break;
                            }
                        }

                        if (handled)
                            break;
                    }
                }
            });
        }
    }
}